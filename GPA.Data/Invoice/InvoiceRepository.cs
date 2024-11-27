using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Dtos.Audit;
using GPA.Dtos.Invoice;
using GPA.Entities.General;
using GPA.Entities.Unmapped.Inventory;
using GPA.Entities.Unmapped.Invoice;
using GPA.Utils;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GPA.Data.Invoice
{
    public interface IInvoiceRepository : IRepository<GPA.Common.Entities.Invoice.Invoice>
    {
        Task<RawInvoice?> GetInvoiceByIdAsync(Guid id);
        Task<IEnumerable<RawInvoice>> GetInvoicesAsync(RequestFilterDto filter);
        Task<int> GetInvoicesCountAsync(RequestFilterDto filter);
        Task UpdateAsync(GPA.Common.Entities.Invoice.Invoice model, IEnumerable<InvoiceDetails> invoiceDetails);
        Task CancelAsync(Guid id, Guid updatedBy);
        Task<List<RawInvoiceDetails>> GetInvoiceDetailsByInvoiceIdAsync(Guid invoiceId);
        Task AddHistory(GPA.Common.Entities.Invoice.Invoice invoice, ICollection<InvoiceDetails> invoiceDetails, string action, Guid by);
    }

    public class InvoiceRepository : Repository<GPA.Common.Entities.Invoice.Invoice>, IInvoiceRepository
    {
        private ILogger<InvoiceRepository> _logger;
        public InvoiceRepository(GPADbContext _dbContext, ILogger<InvoiceRepository> logger) : base(_dbContext)
        {
            _logger = logger;
        }


        public async Task UpdateAsync(GPA.Common.Entities.Invoice.Invoice model, IEnumerable<InvoiceDetails> invoiceDetails)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await _context.InvoiceDetails.Where(x => x.InvoiceId == model.Id).ExecuteDeleteAsync();

                _context.InvoiceDetails.AddRange(invoiceDetails);

                _context.Update(model);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _context.Entry(model).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelAsync(Guid id, Guid updatedBy)
        {
            using var transaction = _context.Database.BeginTransaction();

            var invoice = await _context
                .Invoices
                .Include(x => x.InvoiceDetails)
                .ThenInclude(x => x.InvoiceDetailsAddons)
                .FirstAsync(x => x.Id == id);

            if (invoice is not null && invoice.Status == InvoiceStatus.Saved)
            {
                var relatedProducts = await GetStockDetailsRelatedProductsByInvoiceIdAsync(id);

                await _context.Invoices.Where(x => x.Id == id).ExecuteUpdateAsync(
                    setter => setter
                        .SetProperty(x => x.Status, InvoiceStatus.Cancel)
                        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow)
                        .SetProperty(x => x.UpdatedBy, updatedBy)
                );

                var stock = new Stock
                {
                    TransactionType = TransactionType.Input,
                    Description = "Devolución de factura",
                    Date = DateTime.Now,
                    ReasonId = (int)ReasonTypes.Return,
                    Status = StockStatus.Saved,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = updatedBy,
                    StockDetails = invoice.InvoiceDetails.Select(x => new StockDetails
                    {
                        Quantity = x.Quantity,
                        ProductId = x.ProductId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = updatedBy
                    }).ToList()
                };

                var relatedProductsStock = new Stock
                {
                    TransactionType = TransactionType.Input,
                    Description = "Productos asociados a productos de factura",
                    Date = DateTime.Now,
                    ReasonId = (int)ReasonTypes.Return,
                    Status = StockStatus.Saved,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = updatedBy,
                    StockDetails = relatedProducts.Select(x => new StockDetails
                    {
                        Quantity = x.Quantity,
                        ProductId = x.ProductId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = updatedBy
                    }).ToList()
                };

                try
                {
                    _context.Stocks.Add(stock);

                    if (relatedProducts.Any())
                    {
                        _context.Stocks.Add(relatedProductsStock);
                    }
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await AddHistory(invoice, invoice.InvoiceDetails, ActionConstants.Canceled, updatedBy);
                    _logger.LogInformation("Cancelando factura: {InvoiceId}", id);
                    _logger.LogInformation("Generando entrada: {StockId}", stock.Id);
                    _logger.LogInformation("Generando entrada: {StockId}", relatedProductsStock.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cancelando factura: {InvoiceId}", id);
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<RawStockDetailsRelatedProduct>> GetStockDetailsRelatedProductsByInvoiceIdAsync(Guid invoiceId)
        {
            var query = @$"
                SELECT 
	                ProductId,
	                Quantity
                FROM Inventory.StockDetails 
                WHERE StockId = (
	                SELECT TOP 1 Id FROM Inventory.Stocks 
	                WHERE InvoiceId = @InvoiceId and ReasonId = 7
                )
                    ";

            return await _context.Database.SqlQueryRaw<RawStockDetailsRelatedProduct>(query, new SqlParameter("@InvoiceId", invoiceId)).ToListAsync();
        }

        public async Task<RawInvoice?> GetInvoiceByIdAsync(Guid id)
        {
            var query = @$"
                SELECT 
	                 INV.[Id]
                    ,INV.[Type]
                    ,INV.[Status]
                    ,INV.Code
                    ,INV.[Payment]
                    ,INV.[PaymentStatus]
                    ,INV.[PaymentMethod]
                    ,INV.[Date]
                    ,INV.[Note]
                    ,INV.[ClientId]
                    ,'' AS ClientName
                    ,CONCAT(USR1.FirstName, ' ', USR1.LastName) CreatedByName
	                ,CONCAT(USR2.FirstName, ' ', USR2.LastName) UpdatedByName
                FROM [GPA].[Invoice].[Invoices] INV
	                LEFT JOIN [GPA].[Security].[Users] USR1 ON USR1.Id = INV.CreatedBy
                    LEFT JOIN [GPA].[Security].[Users] USR2 ON USR2.Id = INV.UpdatedBy
                WHERE INV.[Id] = @Id
                    ";

            return await _context.Database.SqlQueryRaw<RawInvoice>(query, new SqlParameter("@Id", id)).FirstOrDefaultAsync();
        }

        public async Task<List<RawInvoiceDetails>> GetInvoiceDetailsByInvoiceIdAsync(Guid invoiceId)
        {
            var query = @$"
                SELECT 
	                 [Id]
                    ,[Price]
                    ,[Quantity]
                    ,[ProductId]
                    ,[InvoiceId]
                FROM [GPA].[Invoice].[InvoiceDetails]
                WHERE [InvoiceId] = @InvoiceId";
            return await _context.Database.SqlQueryRaw<RawInvoiceDetails>(query, new SqlParameter("@InvoiceId", invoiceId)).ToListAsync();
        }

        public async Task<IEnumerable<RawInvoice>> GetInvoicesAsync(RequestFilterDto filter)
        {
            var (termFilter, dateFilter, statusFilter, typeFilter, invoiceListFilter) = GetInvoiceFilter(filter);

            var query = @$"
                SELECT 
                     INV.[Id]
                    ,INV.[Type]
                    ,INV.[Status]
                    ,INV.Code  
                    ,INV.[Payment]
                    ,INV.[PaymentStatus]
                    ,INV.[PaymentMethod]
                    ,INV.[Date]
                    ,INV.[Note]
                    ,INV.[ClientId]
                    ,'' AS ClientName
                    ,INV.[CreatedBy]
	                ,CONCAT(USR1.FirstName, ' ', USR1.LastName) CreatedByName
	                ,CONCAT(USR2.FirstName, ' ', USR2.LastName) UpdatedByName
                FROM [GPA].[Invoice].[Invoices] INV
	                JOIN [GPA].[Invoice].[Clients] CL ON INV.ClientId = CL.Id
	                LEFT JOIN [GPA].[Security].[Users] USR1 ON USR1.Id = INV.CreatedBy
                    LEFT JOIN [GPA].[Security].[Users] USR2 ON USR2.Id = INV.UpdatedBy
                WHERE 1 = 1 
                    {termFilter}
                    {dateFilter}
                    {statusFilter}
                    {typeFilter}
                ORDER BY Id DESC
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
                    ";

            var (Page, PageSize, _) = PagingHelper.GetPagingParameter(filter);
            var (from, to, status, saleType, term) = GetInvoiceFilterParameter(invoiceListFilter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);

            if (termFilter is { Length: > 0 })
            {
                parameters.Add(term);
            }

            if (dateFilter is { Length: > 0 })
            {
                parameters.AddRange([from, to]);
            }

            if (statusFilter is { Length: > 0 })
            {
                parameters.Add(status);
            }

            if (typeFilter is { Length: > 0 })
            {
                parameters.Add(saleType);
            }

            return await _context.Database.SqlQueryRaw<RawInvoice>(query, parameters.ToArray()).ToListAsync();
        }

        public async Task<int> GetInvoicesCountAsync(RequestFilterDto filter)
        {
            var (termFilter, dateFilter, statusFilter, typeFilter, invoiceListFilter) = GetInvoiceFilter(filter);

            var query = @$"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Invoice].[Invoices] INV
	                JOIN [GPA].[Invoice].[Clients] CL ON INV.ClientId = CL.Id
                WHERE 1 = 1 
                    {termFilter}
                    {dateFilter}
                    {statusFilter}
                    {typeFilter}
                    ";

            var (from, to, status, saleType, term) = GetInvoiceFilterParameter(invoiceListFilter);
            var parameters = new List<SqlParameter>();

            if (termFilter is { Length: > 0 })
            {
                parameters.Add(term);
            }

            if (dateFilter is { Length: > 0 })
            {
                parameters.AddRange([from, to]);
            }

            if (statusFilter is { Length: > 0 })
            {
                parameters.Add(status);
            }

            if (typeFilter is { Length: > 0 })
            {
                parameters.Add(saleType);
            }

            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        public async Task AddHistory(GPA.Common.Entities.Invoice.Invoice invoice, ICollection<InvoiceDetails> invoiceDetails, string action, Guid by)
        {
            var query = @"
               INSERT INTO [Audit].[InvoiceHistory]
                   ([Type]
                   ,[Status]
                   ,[Payment]
                   ,[Code]
                   ,[PaymentStatus]
                   ,[PaymentMethod]
                   ,[Date]
                   ,[Note]
                   ,[ClientId]
                   ,[InvoiceDetailsHistory]
                   ,[IdentityId]
                   ,[Action]
                   ,[By]
                   ,[At])
             VALUES(
                    @Type
                   ,@Status
                   ,@Payment
                   ,@Code
                   ,@PaymentStatus
                   ,@PaymentMethod
                   ,@Date
                   ,@Note
                   ,@ClientId
                   ,@InvoiceDetailsHistory
                   ,@IdentityId
                   ,@Action
                   ,@By
                   ,@At)
                    ";

            var detailsWithAddons = invoiceDetails.Select(x => new
            {
                Price = x.Price,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Addon = x.InvoiceDetailsAddons?.Select(add => new
                {
                    Concept = add.Concept,
                    IsDiscount = add.IsDiscount,
                    Type = add.Type,
                    Value = add.Value
                }) ?? []
            });

            var parameters = new SqlParameter[]
            {
                new("@Type", invoice.Type)
               ,new("@Status", invoice.Status)
               ,new("@Payment", invoice.Payment)
               ,new("@Code", invoice.Code)
               ,new("@PaymentStatus", invoice.PaymentStatus)
               ,new("@PaymentMethod", invoice.PaymentMethod)
               ,new("@Date", invoice.Date)
               ,new("@Note", invoice.Note ?? "")
               ,new("@ClientId", invoice.ClientId)
               ,new("@InvoiceDetailsHistory", JsonSerializer.Serialize(detailsWithAddons))
               ,new("@IdentityId", invoice.Id)
               ,new("@Action", action)
               ,new("@By", by)
               ,new("@At", DateTimeOffset.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        }

        private (SqlParameter from, SqlParameter to, SqlParameter status, SqlParameter saleType, SqlParameter term) GetInvoiceFilterParameter(InvoiceFilterDto? invoiceListFilter)
        {
            var from = new SqlParameter("@From", DetailedDateUtil.GetDetailedDate(invoiceListFilter?.From));
            var to = new SqlParameter("@To", DetailedDateUtil.GetDetailedDate(invoiceListFilter?.To));
            var status = new SqlParameter("@Status", invoiceListFilter?.Status == -1 ? null : invoiceListFilter?.Status);
            var saleType = new SqlParameter("@Type", invoiceListFilter?.SaleType == -1 ? null : invoiceListFilter?.SaleType);
            var term = new SqlParameter("@Term", invoiceListFilter?.Term is { Length: > 0 } ? invoiceListFilter?.Term : null);

            from.DbType = System.Data.DbType.Date;
            to.DbType = System.Data.DbType.Date;
            status.DbType = System.Data.DbType.Byte;
            saleType.DbType = System.Data.DbType.Byte;
            term.DbType = System.Data.DbType.String;

            return (from, to, status, saleType, term);
        }

        private (string termFilter, string dateFilter, string statusFilter, string typeFilter, InvoiceFilterDto? invoiceListFilter) GetInvoiceFilter(RequestFilterDto filter)
        {
            var search = SearchHelper.ConvertSearchToString(filter);
            var invoiceListFilter = new InvoiceFilterDto()
            {
                Term = "",
                From = null,
                To = null,
                Status = -1,
                SaleType = -1
            };

            if (search is { Length: > 0 })
            {
                invoiceListFilter = JsonSerializer.Deserialize<InvoiceFilterDto>(search, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var termFilter = invoiceListFilter?.Term is { Length: > 0 } ? $"AND (CONCAT(CL.Name, ' ', CL.LastName) LIKE CONCAT('%', @Term, '%') OR INV.[Code] LIKE CONCAT('%', @Term, '%'))" : "";
            var dateFilter = invoiceListFilter?.From is null || invoiceListFilter?.To is null ? "" : $"AND INV.[Date] BETWEEN @From AND @To";
            var statusFilter = invoiceListFilter?.Status == -1 ? "" : "AND INV.[Status] = @Status";
            var typeFilter = invoiceListFilter?.SaleType == -1 ? "" : "AND INV.[Type] = @Type";
            return (termFilter, dateFilter, statusFilter, typeFilter, invoiceListFilter);
        }
    }
}

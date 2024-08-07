using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Dtos.Invoice;
using GPA.Entities.General;
using GPA.Entities.Unmapped.Invoice;
using GPA.Utils;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        Task<RawInvoiceDetails?> GetInvoiceDetailsByInvoiceIdAsync(Guid invoiceId);
    }

    public class InvoiceRepository : Repository<GPA.Common.Entities.Invoice.Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(GPADbContext _dbContext) : base(_dbContext)
        {
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
                .FirstAsync(x => x.Id == id);

            if (invoice is not null && invoice.Status == InvoiceStatus.Saved)
            {
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

                try
                {

                    _context.Stocks.Add(stock);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<RawInvoice?> GetInvoiceByIdAsync(Guid id)
        {
            var query = @$"
                SELECT 
	                 INV.[Id]
                    ,INV.[Type]
                    ,INV.[Status]
                    ,INV.[Payment]
                    ,INV.[PaymentStatus]
                    ,INV.[Date]
                    ,INV.[Note]
                    ,INV.[ClientId]
                FROM [GPA].[Invoice].[Invoices] INV
                WHERE INV.[Id] = @Id
                    ";

            return await _context.Database.SqlQueryRaw<RawInvoice>(query, new SqlParameter("@Id", id)).FirstOrDefaultAsync();
        }

        public async Task<RawInvoiceDetails?> GetInvoiceDetailsByInvoiceIdAsync(Guid invoiceId)
        {
            var query = @$"
                SELECT 
	                 [Id]
                    ,[Price]
                    ,[Quantity]
                    ,[ProductId]
                    ,[InvoiceId]
                FROM [GPA].[Invoice].[InvoiceDetails]
                WHERE INV.[Id] = @InvoiceId";
            return await _context.Database.SqlQueryRaw<RawInvoiceDetails>(query, new SqlParameter("@InvoiceId", invoiceId)).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawInvoice>> GetInvoicesAsync(RequestFilterDto filter)
        {
            var (termFilter, dateFilter, statusFilter, typeFilter, invoiceListFilter) = GetInvoiceFilter(filter);

            var query = @$"
                SELECT 
	                 INV.[Id]
                    ,INV.[Type]
                    ,INV.[Status]
                    ,INV.[Payment]
                    ,INV.[PaymentStatus]
                    ,INV.[Date]
                    ,INV.[Note]
                    ,INV.[ClientId]
                FROM [GPA].[Invoice].[Invoices] INV
	                JOIN [GPA].[Invoice].[Clients] CL ON INV.ClientId = CL.Id
                WHERE 1 = 1 
                    {termFilter}
                    {dateFilter}
                    {statusFilter}
                    {typeFilter}
                ORDER BY Id
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

            var termFilter = invoiceListFilter?.Term is { Length: > 0 } ? $"AND CONCAT(CL.Name, ' ', CL.LastName) LIKE CONCAT('%', @Term, '%')" : "";
            var dateFilter = invoiceListFilter?.From is null || invoiceListFilter?.To is null ? "" : $"AND INV.[Date] BETWEEN @From AND @To";
            var statusFilter = invoiceListFilter?.Status == -1 ? "" : "AND INV.[Status] = @Status";
            var typeFilter = invoiceListFilter?.SaleType == -1 ? "" : "AND INV.[Type] = @Type";
            return (termFilter, dateFilter, statusFilter, typeFilter, invoiceListFilter);
        }
    }
}

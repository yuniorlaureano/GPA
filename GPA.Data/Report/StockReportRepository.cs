using GPA.Common.DTOs;
using GPA.Dtos.Inventory;
using GPA.Dtos.Invoice;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using GPA.Entities.Unmapped.Invoice;
using GPA.Utils;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GPA.Data.Inventory
{
    public interface IStockReportRepository
    {
        Task<IEnumerable<Existence>> GetAllExistenceAsync(RequestFilterDto filter);
        Task<IEnumerable<RawStock>> GetTransactionsAsync(RequestFilterDto filter);
        Task<IEnumerable<RawAllInvoice>> GetAllInvoicesAsync(RequestFilterDto filter);
    }

    public class StockReportRepository : IStockReportRepository
    {
        private readonly GPADbContext _context;

        public StockReportRepository(GPADbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Existence>> GetAllExistenceAsync(RequestFilterDto filter)
        {
            var (existenceFilterDto, termFilter, typeFilter) = SetExistenceFilterParametersIfNotEmpty(filter);

            var sqlQuery = @$"SELECT 
	                            SUM(CASE
			                            WHEN [t0].[TransactionType] IS NULL THEN 0
			                            WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                            ELSE [t].[Quantity]
	                            END) AS [Stock],
	                            SUM(CASE
                                        WHEN [t0].[TransactionType] IS NULL THEN 0
			                            WHEN [t0].[TransactionType] = 0 THEN [t].[Quantity]
			                            ELSE 0
	                            END) AS [Input],
	                            SUM(CASE
                                        WHEN [t0].[TransactionType] IS NULL THEN 0
			                            WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                            ELSE 0
	                            END) AS [Output],
	                            MAX([p].[Price]) AS [Price],
	                            MAX([P].[Type]) AS [ProductType],
	                            [p].[CategoryId],
	                            [p].[Code] AS [ProductCode],
	                            concat([p].[Name], ' ' , p.UnitValue, ' ' ,UNT.[Name] ) AS [ProductName],
	                            [p].[Id] AS [ProductId]
                            FROM 
	                            [Inventory].[Products] AS [p]
                                JOIN [GPA].[General].[Units] UNT ON p.UnitId = UNT.Id
	                            LEFT JOIN [Inventory].[StockDetails] [t] ON [p].[Id] = [t].[ProductId] AND [t].[Deleted] = CAST(0 AS bit)
	                            LEFT JOIN [Inventory].[Stocks] AS [t0] 
		                            ON [t].[StockId] = [t0].[Id] AND 
		                                [t0].[Deleted] = CAST(0 AS bit) AND 
		                                ([t0].[Status] <> 0 OR [t0].[Status] IS NULL) AND 
		                                ([t0].[Status] <> 2 OR [t0].[Status] IS NULL)
                            WHERE 
	                            [p].[Deleted] = CAST(0 AS bit) 
                                {termFilter}
                                {typeFilter}
                            GROUP BY 
	                            [p].[Id], 
	                            [p].[Name], 
	                            [p].[Code], 
	                            [p].[CategoryId],
                                p.UnitValue,
								UNT.[Name]";

            var parameters = new List<SqlParameter>();
            AddExistenceFilterParameters(existenceFilterDto, termFilter, typeFilter, parameters);

            return await _context.Database.SqlQueryRaw<Existence>(sqlQuery, parameters.ToArray()).ToListAsync();
        }

        public async Task<IEnumerable<RawStock>> GetTransactionsAsync(RequestFilterDto filter)
        {
            var (transactionsFilterDto, dateFilter, termFilter, statusFilter, transactionTypeFilter, reasonFilter) = SetStockFilterParametersIfNotEmpty(filter);
            var query = @$"
                SELECT 
	                 ST.[Id]
                    ,ST.[TransactionType]
                    ,ST.[Description]
                    ,ST.[Date]
                    ,ST.[Status]
                    ,ST.[ProviderId]
                    ,ST.[StoreId]
                    ,ST.[ReasonId]
                    ,ST.[InvoiceId]
	                ,PROV.[Name] ProviderName
	                ,PROV.Identification ProviderIdentification
	                ,RS.[Name] ReasonName
	                ,STRS.[Name] StoreName
                FROM [GPA].[Inventory].[Stocks] ST
	                LEFT JOIN [GPA].[Inventory].[Providers] PROV ON ST.ProviderId = PROV.Id
	                JOIN [GPA].[Inventory].[Reasons] RS ON ST.ReasonId = RS.Id
	                LEFT JOIN [GPA].[Inventory].[Stores] STRS ON ST.StoreId = STRS.Id
                WHERE 1 = 1  
                    {termFilter}
                    {statusFilter}
                    {transactionTypeFilter}
                    {reasonFilter} 
                    {dateFilter} 
            ";

            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            AddStockFilterParameters(transactionsFilterDto, termFilter, statusFilter, transactionTypeFilter, reasonFilter, parameters);

            return await _context.Database.SqlQueryRaw<RawStock>(
                query, parameters.ToArray()
                ).ToListAsync();
        }

        public async Task<IEnumerable<RawAllInvoice>> GetAllInvoicesAsync(RequestFilterDto filter)
        {
            var (termFilter, dateFilter, statusFilter, typeFilter, invoiceListFilter) = GetInvoiceFilter(filter);

            var query = @$"
                SELECT 
	                 INV.[Id]
                    ,INV.[Type]
                    ,INV.[Status]
                    ,INV.[Code]
                    ,INV.[Payment]
                    ,INV.[PaymentStatus]
                    ,INV.[Date]
                    ,INV.[Note]
                    ,INV.[ClientId]
                    ,CL.[Name] AS ClientName
                    ,CL.[LastName] AS ClientLastName 
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

            return await _context.Database.SqlQueryRaw<RawAllInvoice>(query, parameters.ToArray()).ToListAsync();
        }

        private (TransactionsFilterDto? transactionsFilterDto, string dateFilter, string termFilter, string statusFilter, string transactionTypeFilter, string reasonFilter) SetStockFilterParametersIfNotEmpty(RequestFilterDto filter)
        {
            var transactionsFilterDto = new TransactionsFilterDto()
            {
                From = null,
                To = null,
            };
            if (filter.Search is { Length: > 0 })
            {
                transactionsFilterDto = JsonSerializer.Deserialize<TransactionsFilterDto>(SearchHelper.ConvertSearchToString(filter), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var termFilter = transactionsFilterDto?.Term is { Length: > 0 } ? "AND PROV.[Name] LIKE CONCAT('%', @Term, '%')" : "";
            var statusFilter = transactionsFilterDto?.Status != -1 ? "AND ST.[Status] = @Status" : "";
            var transactionTypeFilter = transactionsFilterDto?.TransactionType != -1 ? "AND ST.[TransactionType] = @TransactionType" : "";
            var reasonFilter = transactionsFilterDto?.Reason != -1 ? "AND ST.[ReasonId] = @Reason" : "";
            var dateFilter = transactionsFilterDto?.From is null || transactionsFilterDto?.To is null ? "" : $"AND ST.[Date] BETWEEN @From AND @To";

            return (transactionsFilterDto, dateFilter, termFilter, statusFilter, transactionTypeFilter, reasonFilter);
        }

        private void AddStockFilterParameters(TransactionsFilterDto? transactionsFilterDto, string termFilter, string statusFilter, string transactionTypeFilter, string reasonFilter, List<SqlParameter> parameters)
        {
            if (termFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Term", transactionsFilterDto?.Term));
            }

            if (statusFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Status", transactionsFilterDto?.Status));
            }

            if (transactionTypeFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@TransactionType", transactionsFilterDto?.TransactionType));
            }

            if (reasonFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Reason", transactionsFilterDto?.Reason));
            }

            if (transactionsFilterDto?.From is not null && transactionsFilterDto?.To is not null)
            {
                parameters.Add(new SqlParameter("@From", DetailedDateUtil.GetDetailedDate(transactionsFilterDto?.From)));
                parameters.Add(new SqlParameter("@To", DetailedDateUtil.GetDetailedDate(transactionsFilterDto?.To)));
            }
        }

        private (ExistenceFilterDto? existenceFilterDto, string termFilter, string typeFilter) SetExistenceFilterParametersIfNotEmpty(RequestFilterDto filter)
        {
            var existenceFilterDto = new ExistenceFilterDto();
            if (filter.Search is { Length: > 0 })
            {
                existenceFilterDto = JsonSerializer.Deserialize<ExistenceFilterDto>(SearchHelper.ConvertSearchToString(filter), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var termFilter = existenceFilterDto?.Term is { Length: > 0 } ? "AND ([p].[Code] LIKE CONCAT('%', @Term, '%') OR [p].[Name] LIKE CONCAT('%', @Term, '%'))" : "";
            var typeFilter = existenceFilterDto?.Type != -1 && existenceFilterDto?.Type is not null ? "AND [P].[Type] = @Type" : "";

            return (existenceFilterDto, termFilter, typeFilter);
        }

        private void AddExistenceFilterParameters(ExistenceFilterDto? existenceFilterDto, string termFilter, string typeFilter, List<SqlParameter> parameters)
        {
            if (termFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Term", existenceFilterDto?.Term));
            }

            if (typeFilter is { Length: > 0 })
            {
                parameters.Add(new SqlParameter("@Type", existenceFilterDto?.Type));
            }
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
    }
}

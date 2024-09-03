using GPA.Common.DTOs;
using GPA.Common.Entities.Inventory;
using GPA.Dtos.Inventory;
using GPA.Entities.General;
using GPA.Entities.Unmapped;
using GPA.Entities.Unmapped.Inventory;
using GPA.Utils.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GPA.Data.Inventory
{
    public interface IStockRepository : IRepository<Stock>
    {
        Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(RequestFilterDto filter);
        Task<int> GetProductCatalogCountAsync(RequestFilterDto filter);
        Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(Guid[] productIds);
        Task UpdateAsync(Stock model, IEnumerable<StockDetails> stockDetails);
        Task<IEnumerable<Existence>> GetExistenceAsync(RequestFilterDto filter);
        Task<int> GetExistenceCountAsync(RequestFilterDto filter);
        Task CancelAsync(Guid id, Guid updatedBy);
        Task<IEnumerable<RawStockDetails>> GetStockDetailsByStockIdAsync(Guid stockId);
        Task<RawStock?> GetStockByIdAsync(Guid id);
        Task<IEnumerable<RawStock>> GetStocksAsync(RequestFilterDto filter);
        Task<int> GetStocksCountAsync(RequestFilterDto filter);
    }

    public class StockRepository : Repository<Stock>, IStockRepository
    {
        public StockRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(RequestFilterDto filter)
        {
            var sqlQuery = @"SELECT 
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
	                            [p].[Name] AS [ProductName],
	                            [p].[Id] AS [ProductId]
                            FROM 
	                            [Inventory].[Products] AS [p]
	                            LEFT JOIN [Inventory].[StockDetails] [t] ON [p].[Id] = [t].[ProductId] AND [t].[Deleted] = CAST(0 AS bit)
	                            LEFT JOIN [Inventory].[Stocks] AS [t0] 
		                            ON [t].[StockId] = [t0].[Id] AND 
		                               [t0].[Deleted] = CAST(0 AS bit) AND 
		                               ([t0].[Status] <> 0 OR [t0].[Status] IS NULL) AND 
		                               ([t0].[Status] <> 2 OR [t0].[Status] IS NULL)
                            WHERE 
	                            [p].[Deleted] = CAST(0 AS bit) AND
                                (
                                         @Search IS NULL
	                                  OR [p].[Code] LIKE CONCAT('%', @Search, '%')
	                                  OR [p].[Name] LIKE CONCAT('%', @Search, '%')
                                ) AND
	                            [p].[Type] = @Type 
                            GROUP BY 
	                            [p].[Id], 
	                            [p].[Name], 
	                            [p].[Code], 
	                            [p].[CategoryId]
                            ORDER BY 
	                            [p].[Name]
                            OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            var type = new SqlParameter("@Type", (byte)ProductType.FinishedProduct);
            var parameters = new List<SqlParameter>()
            {
                Page, PageSize, Search, type
            };
            return await _context.Database.SqlQueryRaw<RawProductCatalog>(sqlQuery, parameters.ToArray()).ToListAsync();
        }

        public async Task<int> GetProductCatalogCountAsync(RequestFilterDto filter)
        {
            var query = @"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[Products] PRO
                WHERE PRO.Deleted = 0 AND PRO.[Type] = @Type AND (
	                @Search IS NULL
	                OR PRO.[Code] LIKE CONCAT('%', @Search, '%')
	                OR PRO.[Name] LIKE CONCAT('%', @Search, '%'))
            ";
            var (_, _, Search) = PagingHelper.GetPagingParameter(filter);
            var type = new SqlParameter("@Type", (byte)ProductType.FinishedProduct);
            return await _context.Database.SqlQueryRaw<int>(query, [Search, type]).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Existence>> GetExistenceAsync(RequestFilterDto filter)
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
	                            [p].[Name] AS [ProductName],
	                            [p].[Id] AS [ProductId]
                            FROM 
	                            [Inventory].[Products] AS [p]
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
	                            [p].[CategoryId]
                            ORDER BY 
	                            [p].[Name]
                            OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);
            AddExistenceFilterParameters(existenceFilterDto, termFilter, typeFilter, parameters);

            return await _context.Database.SqlQueryRaw<Existence>(sqlQuery, parameters.ToArray()).ToListAsync();
        }

        public async Task<int> GetExistenceCountAsync(RequestFilterDto filter)
        {
            var (existenceFilterDto, termFilter, typeFilter) = SetExistenceFilterParametersIfNotEmpty(filter);

            var query = @$"
            SELECT 
	            COUNT(1) AS [Value]
            FROM [GPA].[Inventory].[Products] [P]
            WHERE 
	            [Deleted] = CAST(0 AS bit) 
                {termFilter}
                {typeFilter} 
            ";

            var parameters = new List<SqlParameter>();
            AddExistenceFilterParameters(existenceFilterDto, termFilter, typeFilter, parameters);
            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(Guid[] productIds)
        {
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
	                            [p].[Name] AS [ProductName],
	                            [p].[Id] AS [ProductId]
                            FROM 
	                            [Inventory].[Products] AS [p]
	                            LEFT JOIN [Inventory].[StockDetails] [t] ON [p].[Id] = [t].[ProductId] AND [t].[Deleted] = CAST(0 AS bit)
	                            LEFT JOIN [Inventory].[Stocks] AS [t0] 
		                            ON [t].[StockId] = [t0].[Id] AND 
		                               [t0].[Deleted] = CAST(0 AS bit) AND 
		                               ([t0].[Status] <> 0 OR [t0].[Status] IS NULL) AND 
		                               ([t0].[Status] <> 2 OR [t0].[Status] IS NULL)
                            WHERE 
	                            [p].[Deleted] = CAST(0 AS bit) AND
                                [p].[Id] IN ({string.Join(",", productIds.Select(id => $"'{id}'"))}) AND   
	                            [p].[Type] = {(byte)ProductType.FinishedProduct} 
                            GROUP BY 
	                            [p].[Id], 
	                            [p].[Name], 
	                            [p].[Code], 
	                            [p].[CategoryId]";
            return await _context.Database.SqlQueryRaw<RawProductCatalog>(sqlQuery).ToListAsync();
        }

        public async Task UpdateAsync(Stock model, IEnumerable<StockDetails> stockDetails)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await _context.StockDetails.Where(x => x.StockId == model.Id).ExecuteDeleteAsync();

                _context.StockDetails.AddRange(stockDetails);

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
            var stock = await _context.Stocks.Include(x => x.StockDetails).FirstAsync(x => x.Id == id);
            var canCancel =
                stock.ReasonId != (int)ReasonTypes.Sale &&
                (stock.Status == StockStatus.Draft || stock.Status == StockStatus.Saved);

            if (!canCancel)
            {
                return;
            }

            await _context.Stocks.Where(x => x.Id == id)
                    .ExecuteUpdateAsync(setter =>
                        setter
                            .SetProperty(x => x.Status, StockStatus.Canceled)
                            .SetProperty(x => x.UpdatedAt, DateTimeOffset.Now)
                            .SetProperty(x => x.UpdatedBy, updatedBy)
                        );

            await CreateNewStockForCanceledOutput(updatedBy, stock);

        }

        public async Task<IEnumerable<RawStockDetails>> GetStockDetailsByStockIdAsync(Guid stockId)
        {
            var query = @$"
                SELECT [Id]
                      ,[Quantity]
                      ,[PurchasePrice]
                      ,[ProductId]
                      ,[StockId]
                FROM [GPA].[Inventory].[StockDetails]
                WHERE [StockId] = @StockId 
            ";

            return await _context.Database.SqlQueryRaw<RawStockDetails>(
                query, new SqlParameter("@StockId", stockId)
                ).ToListAsync();
        }

        public async Task<RawStock?> GetStockByIdAsync(Guid id)
        {
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
                WHERE ST.[Id] = @Id 
            ";

            return await _context.Database.SqlQueryRaw<RawStock>(
                query, new SqlParameter("@Id", id)
                ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawStock>> GetStocksAsync(RequestFilterDto filter)
        {
            var (transactionsFilterDto, termFilter, statusFilter, transactionTypeFilter, reasonFilter) = SetStockFilterParametersIfNotEmpty(filter);
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
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);
            AddStockFilterParameters(transactionsFilterDto, termFilter, statusFilter, transactionTypeFilter, reasonFilter, parameters);

            return await _context.Database.SqlQueryRaw<RawStock>(
                query, parameters.ToArray()
                ).ToListAsync();
        }

        public async Task<int> GetStocksCountAsync(RequestFilterDto filter)
        {
            var (transactionsFilterDto, termFilter, statusFilter, transactionTypeFilter, reasonFilter) = SetStockFilterParametersIfNotEmpty(filter);
            var query = @$"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[Stocks] ST
                    LEFT JOIN [GPA].[Inventory].[Providers] PROV ON ST.ProviderId = PROV.Id 
                WHERE 1 = 1  
                    {termFilter}
                    {statusFilter}
                    {transactionTypeFilter}
                    {reasonFilter}
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);
            AddStockFilterParameters(transactionsFilterDto, termFilter, statusFilter, transactionTypeFilter, reasonFilter, parameters);

            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        public async Task SavePhoto(string fullFileName, Guid productId)
        {
            var query = @$"
                UPDATE [GPA].[Inventory].[Products]
                SET Photo = @Photo 
                WHERE Id = @Id 
            ";

            await _context.Database.ExecuteSqlRawAsync(query, new SqlParameter("@Photo", fullFileName), new SqlParameter("@Id", productId));
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

        private (TransactionsFilterDto? transactionsFilterDto, string termFilter, string statusFilter, string transactionTypeFilter, string reasonFilter) SetStockFilterParametersIfNotEmpty(RequestFilterDto filter)
        {
            var transactionsFilterDto = new TransactionsFilterDto();
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

            return (transactionsFilterDto, termFilter, statusFilter, transactionTypeFilter, reasonFilter);
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
        }

        private async Task CreateNewStockForCanceledOutput(Guid updatedBy, Stock stock)
        {
            if (stock.TransactionType == TransactionType.Output)
            {
                //when cancelling an output, we need to create a new input with the same details
                var newStock = new Stock
                {
                    TransactionType = TransactionType.Input,
                    Description = "Cancelación de salida",
                    Date = DateTime.UtcNow,
                    ReasonId = (int)ReasonTypes.OutputCancellation,
                    Status = StockStatus.Saved,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = updatedBy,
                    StockDetails = stock.StockDetails.Select(x => new StockDetails
                    {
                        Quantity = x.Quantity,
                        ProductId = x.ProductId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = updatedBy
                    }).ToList()
                };

                _context.Stocks.Add(newStock);
                await _context.SaveChangesAsync();
            }
        }
    }
}

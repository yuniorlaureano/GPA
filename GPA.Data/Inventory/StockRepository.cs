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
        Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(int page = 1, int pageSize = 10);
        Task<int> GetProductCatalogCountAsync();
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

        public async Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(int page = 1, int pageSize = 10)
        {
            var sqlQuery = @"SELECT 
	                            SUM(CASE
			                            WHEN [t].[Id] IS NULL THEN 0
			                            WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                            ELSE [t].[Quantity]
	                            END) AS [Stock],
	                            SUM(CASE
			                            WHEN [t0].[TransactionType] = 0 THEN [t].[Quantity]
			                            ELSE 0
	                            END) AS [Input],
	                            SUM(CASE
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
	                            [p].[Type] = {2} 
                            GROUP BY 
	                            [p].[Id], 
	                            [p].[Name], 
	                            [p].[Code], 
	                            [p].[CategoryId]
                            ORDER BY 
	                            [p].[Name]
                            OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";
            var productCatalog = await _context.Database.SqlQueryRaw<RawProductCatalog>(
                    sqlQuery,
                    pageSize * Math.Abs(page - 1),
                    pageSize,
                    (byte)ProductType.FinishedProduct
                ).ToListAsync();

            return productCatalog;
        }

        public async Task<int> GetProductCatalogCountAsync()
        {
            return await _context.Products.Where(x => x.Type == ProductType.FinishedProduct).CountAsync();
        }

        public async Task<IEnumerable<Existence>> GetExistenceAsync(RequestFilterDto filter)
        {
            var (existenceFilterDto, termFilter, typeFilter) = SetFilterParametersIfNotEmpty(filter);

            var sqlQuery = @$"SELECT 
	                            SUM(CASE
			                            WHEN [t].[Id] IS NULL THEN 0
			                            WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                            ELSE [t].[Quantity]
	                            END) AS [Stock],
	                            SUM(CASE
			                            WHEN [t0].[TransactionType] = 0 THEN [t].[Quantity]
			                            ELSE 0
	                            END) AS [Input],
	                            SUM(CASE
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
            AddFilterParameters(existenceFilterDto, termFilter, typeFilter, parameters);

            return await _context.Database.SqlQueryRaw<Existence>(sqlQuery, parameters.ToArray()).ToListAsync();
        }

        public async Task<int> GetExistenceCountAsync(RequestFilterDto filter)
        {
            var (existenceFilterDto, termFilter, typeFilter) = SetFilterParametersIfNotEmpty(filter);

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
            AddFilterParameters(existenceFilterDto, termFilter, typeFilter, parameters);
            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(Guid[] productIds)
        {
            var sqlQuery = @$"SELECT 
	                            SUM(CASE
			                            WHEN [t].[Id] IS NULL THEN 0
			                            WHEN [t0].[TransactionType] = 1 THEN [t].[Quantity] * -1
			                            ELSE [t].[Quantity]
	                            END) AS [Stock],
	                            SUM(CASE
			                            WHEN [t0].[TransactionType] = 0 THEN [t].[Quantity]
			                            ELSE 0
	                            END) AS [Input],
	                            SUM(CASE
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
            var stock = await _context.Stocks.FirstAsync(x => x.Id == id);
            var canCancel =
                stock.ReasonId != (int)ReasonTypes.Sale &&
                (stock.Status == StockStatus.Draft || stock.Status == StockStatus.Saved);
            if (canCancel)
            {
                await _context.Stocks.Where(x => x.Id == id)
                    .ExecuteUpdateAsync(setter =>
                        setter
                            .SetProperty(x => x.Status, StockStatus.Canceled)
                            .SetProperty(x => x.UpdatedAt, DateTimeOffset.Now)
                            .SetProperty(x => x.UpdatedBy, updatedBy)
                        );
            }
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
	                ,PROV.Identification ProviderRnc
	                ,RS.[Name] ReasonName
	                ,STRS.[Name] StoreName
                FROM [GPA].[Inventory].[Stocks] ST
	                LEFT JOIN [GPA].[Inventory].[Providers] PROV ON ST.ProviderId = PROV.Id
	                JOIN [GPA].[Inventory].[Reasons] RS ON ST.ReasonId = RS.Id
	                JOIN [GPA].[Inventory].[Stores] STRS ON ST.StoreId = STRS.Id
                WHERE ST.[Id] = @Id 
            ";

            return await _context.Database.SqlQueryRaw<RawStock>(
                query, new SqlParameter("@Id", id)
                ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RawStock>> GetStocksAsync(RequestFilterDto filter)
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
	                ,PROV.Identification ProviderRnc
	                ,RS.[Name] ReasonName
	                ,STRS.[Name] StoreName
                FROM [GPA].[Inventory].[Stocks] ST
	                LEFT JOIN [GPA].[Inventory].[Providers] PROV ON ST.ProviderId = PROV.Id
	                JOIN [GPA].[Inventory].[Reasons] RS ON ST.ReasonId = RS.Id
	                JOIN [GPA].[Inventory].[Stores] STRS ON ST.StoreId = STRS.Id
                WHERE 1 = 1  
                ORDER BY Id
                OFFSET @Page ROWS FETCH NEXT @PageSize ROWS ONLY 
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);

            return await _context.Database.SqlQueryRaw<RawStock>(
                query, parameters.ToArray()
                ).ToListAsync();
        }

        public async Task<int> GetStocksCountAsync(RequestFilterDto filter)
        {
            var query = @$"
                SELECT 
	                 COUNT(1) AS [Value]
                FROM [GPA].[Inventory].[Stocks] ST
                WHERE 1 = 1  
            ";

            var (Page, PageSize, Search) = PagingHelper.GetPagingParameter(filter);
            var parameters = new List<SqlParameter>();
            parameters.AddRange([Page, PageSize]);

            return await _context.Database.SqlQueryRaw<int>(query, parameters.ToArray()).FirstOrDefaultAsync();
        }

        private (ExistenceFilterDto? existenceFilterDto, string termFilter, string typeFilter) SetFilterParametersIfNotEmpty(RequestFilterDto filter)
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
            var typeFilter = existenceFilterDto?.Type != 0 ? "AND [P].[Type] = @Type" : "";

            return (existenceFilterDto, termFilter, typeFilter);
        }

        private void AddFilterParameters(ExistenceFilterDto? existenceFilterDto, string termFilter, string typeFilter, List<SqlParameter> parameters)
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
    }
}

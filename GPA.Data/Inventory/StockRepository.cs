using GPA.Common.Entities.Inventory;
using GPA.Entities.Common;
using GPA.Entities.Unmapped;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IStockRepository : IRepository<Stock>
    {
        Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(int page = 1, int pageSize = 10);
        Task<int> GetProductCatalogCountAsync();
        Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(Guid[] productIds);
        Task UpdateAsync(Stock model, IEnumerable<StockDetails> stockDetails);
        Task<IEnumerable<Existence>> GetExistenceAsync(int page = 1, int pageSize = 10);
        Task<int> GetExistenceCountAsync();
        Task CancelAsync(Guid id);
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
	                            [p].[Deleted] = CAST(0 AS bit) 
                            GROUP BY 
	                            [p].[Id], 
	                            [p].[Name], 
	                            [p].[Code], 
	                            [p].[CategoryId]
                            ORDER BY 
	                            [p].[Name]
                            OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";
            return await _context.Database.SqlQueryRaw<RawProductCatalog>(
                    sqlQuery,
                    pageSize * Math.Abs(page - 1),
                    pageSize
                ).ToListAsync();
        }

        public async Task<int> GetProductCatalogCountAsync()
        {
            return await _context.Products.Select(x => x.Id).CountAsync();
        }

        public async Task<IEnumerable<Existence>> GetExistenceAsync(int page = 1, int pageSize = 10)
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
		                                ([t0].[Status] <> 0 OR [t0].[Status] IS NULL) 
                            WHERE 
	                            [p].[Deleted] = CAST(0 AS bit) 
                            GROUP BY 
	                            [p].[Id], 
	                            [p].[Name], 
	                            [p].[Code], 
	                            [p].[CategoryId]
                            ORDER BY 
	                            [p].[Name]
                            OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";
            return await _context.Database.SqlQueryRaw<Existence>(
                    sqlQuery,
                    pageSize * Math.Abs(page - 1),
                    pageSize
                ).ToListAsync();
        }

        public async Task<int> GetExistenceCountAsync()
        {
            return await _context.Products.Select(x => x.Id).CountAsync();
        }


        public async Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(Guid[] productIds)
        {
            var sale = (int)ReasonTypes.Sale;            
            var query = from p in _context.Products
                        join sd in _context.StockDetails on p.Id equals sd.ProductId into sd_jointable
                        from sd_row in sd_jointable.DefaultIfEmpty()
                        join s in _context.Stocks on sd_row.StockId equals s.Id into s_jointable
                        from s_row in s_jointable.DefaultIfEmpty()
                        where productIds.Contains(p.Id) &&
                            s_row.ReasonId != sale &&
                            s_row.Status != StockStatus.Draft &&
                            s_row.Status != StockStatus.Canceled
                        group new { s_row, sd_row, p } by new { p.Id, p.Name, p.Code, p.CategoryId } into g
                        select new RawProductCatalog
                        {
                            Stock = g.Sum(x => x.sd_row == null ? 0 :
                                                (x.s_row.TransactionType == TransactionType.Output ?
                                                                        (x.sd_row.Quantity * -1) : x.sd_row.Quantity)),
                            Price = g.Max(x => x.p.Price),
                            CategoryId = g.Key.CategoryId,
                            ProductCode = g.Key.Code,
                            ProductName = g.Key.Name,
                            ProductId = g.Key.Id
                        };

            return await query.ToListAsync();
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

        public async Task CancelAsync(Guid id)
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
                            .SetProperty(x => x.Status, Entities.Common.StockStatus.Canceled)
                            .SetProperty(x => x.UpdatedAt, DateTime.Now)
                        );
            }
        }
    }
}

using GPA.Common.Entities.Inventory;
using GPA.Entities.Unmapped;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IStockRepository : IRepository<Stock>
    {
        Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(int page = 1, int pageSize = 10);
        Task<int> GetProductCatalogCountAsync();
    }

    public class StockRepository : Repository<Stock>, IStockRepository
    {
        public StockRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task<IEnumerable<RawProductCatalog>> GetProductCatalogAsync(int page = 1, int pageSize = 10)
        {
            var query = from s in _context.Stocks
                        join p in _context.Products on s.ProductId equals p.Id
                        group new { s, p } by new { s.ProductId, p.Name, p.Code, p.CategoryId } into g
                        select new RawProductCatalog
                        {
                            Quantity = g.Sum(x => x.s.TransactionType == Entities.Common.TransactionType.Output ? (x.s.Quantity * -1) : x.s.Quantity),
                            Price = g.Max(x => x.p.Price),
                            CategoryId = g.Key.CategoryId,
                            ProductCode = g.Key.Code,
                            ProductName = g.Key.Name,
                            ProductId = g.Key.ProductId
                        };

            return await query.Skip(pageSize * Math.Abs(page - 1)).Take(pageSize).ToListAsync();
        }

        public async Task<int> GetProductCatalogCountAsync()
        {
            return await _context.Stocks.Select(x => x.ProductId).Distinct().CountAsync();
        }
    }
}

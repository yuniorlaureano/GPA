using GPA.Common.Entities.Inventory;

namespace GPA.Data.Inventory
{
    public interface IStockRepository : IRepository<Stock>
    {
    }

    public class StockRepository : Repository<Stock>, IStockRepository
    {
        public StockRepository(DbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

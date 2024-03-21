using GPA.Common.Entities.Inventory;

namespace GPA.Data.Inventory
{
    public interface IItemRepository : IRepository<Item>
    {
    }

    public class ItemRepository : Repository<Item>, IItemRepository
    {
        public ItemRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

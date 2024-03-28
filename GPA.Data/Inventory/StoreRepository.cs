using GPA.Common.Entities.Inventory;

namespace GPA.Data.Inventory
{
    public interface IStoreRepository : IRepository<Store>
    {
    }

    public class StoreRepository : Repository<Store>, IStoreRepository
    {
        public StoreRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

using GPA.Common.Entities.Inventory;

namespace GPA.Data.Inventory
{
    public interface IProviderAddressRepository : IRepository<ProviderAddress>
    {
    }

    public class ProviderAddressRepository : Repository<ProviderAddress>, IProviderAddressRepository
    {
        public ProviderAddressRepository(DbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

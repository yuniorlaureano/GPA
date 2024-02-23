using GPA.Common.Entities.Inventory;

namespace GPA.Data.Inventory
{
    public interface IProductRepository : IRepository<Product>
    {
    }

    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

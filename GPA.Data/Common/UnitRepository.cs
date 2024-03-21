using GPA.Entities.Common;

namespace GPA.Data.Common
{
    public interface IUnitRepository : IRepository<Unit>
    {
    }

    public class UnitRepository : Repository<Unit>, IUnitRepository
    {
        public UnitRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

using GPA.Entities.General;

namespace GPA.Data.General
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

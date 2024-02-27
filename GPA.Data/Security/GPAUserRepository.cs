using GPA.Common.Entities.Security;

namespace GPA.Data.Security
{
    public interface IGPAUserRepository : IRepository<GPAUser>
    {
    }

    public class GPAUserRepository : Repository<GPAUser>, IGPAUserRepository
    {
        public GPAUserRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

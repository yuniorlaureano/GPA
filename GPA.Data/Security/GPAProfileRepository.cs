using GPA.Common.Entities.Security;

namespace GPA.Data.Security
{
    public interface IGPAProfileRepository : IRepository<GPAProfile>
    {
    }

    public class GPAProfileRepository : Repository<GPAProfile>, IGPAProfileRepository
    {
        public GPAProfileRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

using GPA.Entities.General;

namespace GPA.Data.General
{
    public interface IEmailConfigurationRepository : IRepository<EmailConfiguration>
    {
    }

    public class EmailConfigurationRepository : Repository<EmailConfiguration>, IEmailConfigurationRepository
    {
        public EmailConfigurationRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

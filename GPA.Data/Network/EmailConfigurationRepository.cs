using GPA.Entities.Network;

namespace GPA.Data.Network
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

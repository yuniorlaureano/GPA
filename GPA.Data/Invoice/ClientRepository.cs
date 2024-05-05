using GPA.Common.Entities.Invoice;

namespace GPA.Data.Invoice
{
    public interface IClientRepository : IRepository<Client>
    {
    }

    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

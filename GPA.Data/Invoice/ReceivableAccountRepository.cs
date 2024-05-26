namespace GPA.Data.Invoice
{
    public interface IReceivableAccountRepository : IRepository<GPA.Common.Entities.Invoice.ClientPaymentsDetails>
    {
    }

    public class ReceivableAccountRepository : Repository<GPA.Common.Entities.Invoice.ClientPaymentsDetails>, IReceivableAccountRepository
    {
        public ReceivableAccountRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

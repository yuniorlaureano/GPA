namespace GPA.Data.Invoice
{
    public interface IInvoiceRepository : IRepository<GPA.Common.Entities.Invoice.Invoice>
    {
    }

    public class InvoiceRepository : Repository<GPA.Common.Entities.Invoice.Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }
    }
}

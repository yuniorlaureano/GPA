using GPA.Common.Entities.Invoice;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Invoice
{
    public interface IInvoiceRepository : IRepository<GPA.Common.Entities.Invoice.Invoice>
    {
        Task UpdateAsync(GPA.Common.Entities.Invoice.Invoice model, IEnumerable<InvoiceDetails> invoiceDetails);
    }

    public class InvoiceRepository : Repository<GPA.Common.Entities.Invoice.Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }


        public async Task UpdateAsync(GPA.Common.Entities.Invoice.Invoice model, IEnumerable<InvoiceDetails> invoiceDetails)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await _context.InvoiceDetails.Where(x => x.InvoiceId == model.Id).ExecuteDeleteAsync();

                _context.InvoiceDetails.AddRange(invoiceDetails);

                _context.Update(model);
                                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _context.Entry(model).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }
    }
}

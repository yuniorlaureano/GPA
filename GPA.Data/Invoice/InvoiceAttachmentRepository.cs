using GPA.Common.Entities.Invoice;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IInvoiceAttachmentRepository
    {
        Task<InvoiceAttachment?> GetAttachmentByIdAsync(Guid id);
        Task<IEnumerable<InvoiceAttachment>> GetAttachmentByInvoiceIdAsync(Guid invoiceId);
        Task<IEnumerable<InvoiceAttachment>> GetAttachments();
        Task SaveAttachmentAsync(InvoiceAttachment InvoiceAttachment);
    }

    public class InvoiceAttachmentRepository : IInvoiceAttachmentRepository
    {
        private readonly GPADbContext _context;

        public InvoiceAttachmentRepository(GPADbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceAttachment?> GetAttachmentByIdAsync(Guid id)
        {
            return await _context.InvoiceAttachments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<InvoiceAttachment>> GetAttachmentByInvoiceIdAsync(Guid invoiceId)
        {
            return await _context.InvoiceAttachments.Where(x => x.InvoiceId == invoiceId).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceAttachment>> GetAttachments()
        {
            return await _context.InvoiceAttachments.ToListAsync();
        }

        public async Task SaveAttachmentAsync(InvoiceAttachment InvoiceAttachment)
        {
            if (InvoiceAttachment is null)
            {
                return;
            }

            _context.InvoiceAttachments.Add(InvoiceAttachment);
            await _context.SaveChangesAsync();
        }


    }
}

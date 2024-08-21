using GPA.Entities.Inventory;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Inventory
{
    public interface IStockAttachmentRepository
    {
        Task<StockAttachment?> GetAttachmentByIdAsync(Guid id);
        Task<IEnumerable<StockAttachment>> GetAttachmentByStockIdAsync(Guid stockId);
        Task<IEnumerable<StockAttachment>> GetAttachments();
        Task SaveAttachmentAsync(StockAttachment stockAttachment);
    }

    public class StockAttachmentRepository : IStockAttachmentRepository
    {
        private readonly GPADbContext _context;

        public StockAttachmentRepository(GPADbContext context)
        {
            _context = context;
        }

        public async Task<StockAttachment?> GetAttachmentByIdAsync(Guid id)
        {
            return await _context.StockAttachments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<StockAttachment>> GetAttachmentByStockIdAsync(Guid stockId)
        {
            return await _context.StockAttachments.Where(x => x.StockId == stockId).ToListAsync();
        }

        public async Task<IEnumerable<StockAttachment>> GetAttachments()
        {
            return await _context.StockAttachments.ToListAsync();
        }

        public async Task SaveAttachmentAsync(StockAttachment stockAttachment)
        {
            if (stockAttachment is null)
            {
                return;
            }

            _context.StockAttachments.Add(stockAttachment);
            await _context.SaveChangesAsync();
        }


    }
}

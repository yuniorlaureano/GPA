using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Entities.General;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Invoice
{
    public interface IInvoiceRepository : IRepository<GPA.Common.Entities.Invoice.Invoice>
    {
        Task UpdateAsync(GPA.Common.Entities.Invoice.Invoice model, IEnumerable<InvoiceDetails> invoiceDetails);
        Task CancelAsync(Guid id, Guid updatedBy);
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
                throw;
            }
        }

        public async Task CancelAsync(Guid id, Guid updatedBy)
        {
            using var transaction = _context.Database.BeginTransaction();

            var invoice = await _context
                .Invoices
                .Include(x => x.InvoiceDetails)
                .FirstAsync(x => x.Id == id);

            if (invoice is not null && invoice.Status == InvoiceStatus.Saved)
            {
                await _context.Invoices.Where(x => x.Id == id).ExecuteUpdateAsync(
                    setter => setter
                        .SetProperty(x => x.Status, InvoiceStatus.Cancel)
                        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow)
                        .SetProperty(x => x.UpdatedBy, updatedBy)
                );

                var stock = new Stock
                {
                    TransactionType = TransactionType.Input,
                    Description = "Devolución de factura",
                    Date = DateTime.Now,
                    ReasonId = (int)ReasonTypes.Return,
                    Status = StockStatus.Saved,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = updatedBy,
                    StockDetails = invoice.InvoiceDetails.Select(x => new StockDetails
                    {
                        Quantity = x.Quantity,
                        ProductId = x.ProductId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = updatedBy
                    }).ToList()
                };

                try
                {
                    
                    _context.Stocks.Add(stock);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}

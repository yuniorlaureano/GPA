using GPA.Common.Entities.Inventory;
using GPA.Entities.General;

namespace GPA.Entities.Unmapped.Audit
{
    public class StockHistory : EntityHistory<Guid>
    {
        public TransactionType TransactionType { get; set; }
        public string? Description { get; set; }
        public DateTime? Date { get; set; }
        public StockStatus Status { get; set; }
        public Guid? ProviderId { get; set; }
        public Guid? StoreId { get; set; }
        public int ReasonId { get; set; }
        public Guid? InvoiceId { get; set; }
        public string StockDetailsHistory { get; set; }
    }
}

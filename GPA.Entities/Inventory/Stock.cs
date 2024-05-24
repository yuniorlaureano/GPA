using GPA.Entities;
using GPA.Entities.Common;

namespace GPA.Common.Entities.Inventory
{
    public class Stock : Entity<Guid>
    {
        public TransactionType TransactionType { get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; }
        public StockStatus Status { get; set; }
        public Guid? ProviderId { get; set; }
        public Provider? Provider { get; set; }
        public Guid? StoreId { get; set; }
        public Store? Store { get; set; }
        public int ReasonId { get; set; }
        public Reason Reason { get; set; }

        public ICollection<StockDetails> StockDetails { get; set; }
    }
}

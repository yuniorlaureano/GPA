using GPA.Entities;
using GPA.Entities.Common;

namespace GPA.Common.Entities.Inventory
{
    public class Stock : Entity<Guid>
    {
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }

        public int Quantity { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid? ProviderId { get; set; }
        public Provider? Provider { get; set; }

        public Guid StoreId { get; set; }
        public Store Store { get; set; }

        public int ReasonId { get; set; }
        public Reason Reason { get; set; }
    }
}

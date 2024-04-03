using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class Purchase : Entity<Guid>
    {
        public string Status { get; set; }
        public string Type { get; set; }

        public ICollection<PurchaseDetails> PurchaseDetailses { get; set; }
    }
}

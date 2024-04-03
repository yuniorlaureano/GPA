using GPA.Common.Entities.Invoice;
using GPA.Common.Entities.Security;
using GPA.Entities;

namespace GPA.Common.Entities.Comon
{
    public class Delivery : Entity<Guid>
    {
        public Guid UserId { get; set; }
        public GPAUser User { get; set; }

        public ICollection<InvoiceDelivery> InvoiceDeliveries { get; set; }
    }
}

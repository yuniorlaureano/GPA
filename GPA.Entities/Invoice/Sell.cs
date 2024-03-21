using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class Sell : Entity
    {
        public string Status { get; set; }
        public string Type { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public Invoice Invoice { get; set; }
    }
}

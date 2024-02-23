using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class Purchase : Entity
    {
        public string Status { get; set; }
        public string Type { get; set; }
    }
}

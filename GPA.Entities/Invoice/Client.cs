using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class Client : Entity
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Identification { get; set; }
        public string IdentificationType { get; set; }
        public string Phone { get; set; }
        public decimal AvailableCredit { get; set; }
        public string Email { get; set; }
    }
}

using GPA.Entities;

namespace GPA.Common.Entities.Invoice
{
    public class ClientAddress : Entity
    {
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }
    }
}

using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class Provider : Entity
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Identification { get; set; }
        public string IdentificationType { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public ICollection<Stock> Stocks { get; set; }
    }
}

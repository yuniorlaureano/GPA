using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class ProviderAddress : Entity<Guid>
    {
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; }
    }
}

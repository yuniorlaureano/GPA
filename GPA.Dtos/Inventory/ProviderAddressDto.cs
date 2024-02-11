namespace GPA.Common.DTOs.Inventory
{
    public class ProviderAddressDto
    {
        public Guid? Id { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public Guid ProviderId { get; set; }
    }
}

namespace GPA.Common.DTOs.Inventory
{
    public class ProviderDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string? RNC { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public ICollection<ProviderAddressDto> ProviderAddresses { get; set; }
    }
}

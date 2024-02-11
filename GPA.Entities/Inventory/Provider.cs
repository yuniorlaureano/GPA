using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class Provider : Entity
    {
        public string Name { get; set; }
        public string? RNC { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public ICollection<ProviderAddress> ProviderAddresses { get; set; }
        public ICollection<Stock> Stocks { get; set; }
    }
}

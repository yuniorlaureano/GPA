using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class Reason : Entity<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Stock> Stocks { get; set; }
    }
}

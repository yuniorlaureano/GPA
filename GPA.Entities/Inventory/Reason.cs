using GPA.Entities;
using GPA.Entities.General;

namespace GPA.Common.Entities.Inventory
{
    public class Reason : Entity<int>
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }

        public ICollection<Stock> Stocks { get; set; }
    }
}

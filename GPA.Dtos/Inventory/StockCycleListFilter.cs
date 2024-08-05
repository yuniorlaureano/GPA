using GPA.Common.DTOs.Inventory;

namespace GPA.Dtos.Inventory
{
    public class StockCycleListFilter
    {
        public required string DateTypeFilter { get; set; }
        public DetailedDate? From { get; set; }
        public DetailedDate? To { get; set; }
        public int? IsClose { get; set; }
    }
}

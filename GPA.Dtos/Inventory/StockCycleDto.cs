namespace GPA.Common.DTOs.Inventory
{
    public class StockCycleDto
    {
        public Guid? Id { get; set; }
        public string Note { get; set; }
        public DetailedDate StartDate { get; set; }
        public DetailedDate EndDate { get; set; }
        public Guid? ChildCycleId { get; set; }

        public ICollection<StockCycleDetailDto> StockCycleDetails { get; set; }
    }
}

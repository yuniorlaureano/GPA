namespace GPA.Common.DTOs.Inventory
{
    public class StockCycleDto
    {
        public Guid? Id { get; set; }
        public string Note { get; set; }
        public DetailedDate StartDate { get; set; }
        public DetailedDate EndDate { get; set; }
        public bool IsClose { get; set; }

        public ICollection<StockCycleDetailDto> StockCycleDetails { get; set; }
    }
}

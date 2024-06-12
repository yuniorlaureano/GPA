namespace GPA.Common.DTOs.Inventory
{
    public class StockCycleCreationDto
    {
        public Guid? Id { get; set; }
        public string Note { get; set; }
        public DetailedDate StartDate { get; set; }
        public DetailedDate EndDate { get; set; }
    }
}

namespace GPA.Entities.Unmapped.Inventory
{
    public class RawStockCycle
    {
        public Guid Id { get; set; }
        public string Note { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsClose { get; set; }
    }
}

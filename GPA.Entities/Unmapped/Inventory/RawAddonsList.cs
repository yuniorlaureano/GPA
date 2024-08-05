namespace GPA.Entities.Unmapped.Inventory
{
    public class RawAddonsList
    {
        public Guid? Id { get; set; }
        public string Concept { get; set; }
        public bool IsDiscount { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
    }
}

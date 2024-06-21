namespace GPA.Common.DTOs.Inventory
{
    public class AddonDto
    {
        public Guid? Id { get; set; }
        public string Concept { get; set; }
        public bool IsDiscount { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
    }
}

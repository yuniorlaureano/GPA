namespace GPA.Entities.Unmapped.Audit
{
    public class ProductHistory : EntityHistory<Guid>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public byte Type { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
        public string? ProductLocation { get; set; }
    }
}

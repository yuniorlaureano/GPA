namespace GPA.Entities.Unmapped.Inventory
{
    public class RawProductByAddonId
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }

    }
}

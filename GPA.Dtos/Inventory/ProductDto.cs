namespace GPA.Common.DTOs.Inventory
{
    public class ProductDto
    {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public decimal UnitCost { get; set; }
        public string? Description { get; set; }
        public decimal? WholesaleCost { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? ProductLocationId { get; set; }

        public CategoryDto Category { get; set; }
        public ProductLocationDto? ProductLocation { get; set; }
    }
}

namespace GPA.Common.DTOs.Inventory
{
    public class ProductDto
    {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string BarCode { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public Guid UnitId { get; set; }
        public string Unit { get; set; }

        public Guid CategoryId { get; set; }
        public string Category { get; set; }

        public Guid? ProductLocationId { get; set; }
        public string ProductLocation { get; set; }
    }
}

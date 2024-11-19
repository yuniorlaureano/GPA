namespace GPA.Dtos.Unmapped
{
    public class RawRelatedProductRead
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid RelatedProductId { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
    }
}

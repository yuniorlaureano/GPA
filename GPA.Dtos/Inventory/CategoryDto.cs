namespace GPA.Common.DTOs.Inventory
{
    public class CategoryDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string? Description { get; set; }
    }
}

namespace GPA.Common.DTOs.Inventory
{
    public class StoreDto
    {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string? Description { get; set; }
        public string Location { get; set; }
    }
}

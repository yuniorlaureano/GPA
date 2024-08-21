namespace GPA.Dtos.Inventory
{
    public class StockAttachmentDto
    {
        public Guid Id { get; set; }
        public string File { get; set; }
        public Guid? UploadedBy { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
        public Guid StockId { get; set; }
    }
}

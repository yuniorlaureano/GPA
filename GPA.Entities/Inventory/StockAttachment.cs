using GPA.Common.Entities.Inventory;

namespace GPA.Entities.Inventory
{
    public class StockAttachment
    {
        public Guid Id { get; set; }
        public string File { get; set; }
        public Guid? UploadedBy { get; set; }
        public DateTimeOffset UploadedAt { get; set; }

        public Guid StockId { get; set; }
        public Stock Stock { get; set; }
    }
}

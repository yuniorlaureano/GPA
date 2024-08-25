namespace GPA.Dtos.Invoice
{
    public class InvoiceAttachmentDto
    {
        public Guid Id { get; set; }
        public string File { get; set; }
        public Guid? UploadedBy { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
        public Guid InvoiceId { get; set; }
    }
}

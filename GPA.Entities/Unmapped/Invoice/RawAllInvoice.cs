namespace GPA.Entities.Unmapped.Invoice
{
    public class RawAllInvoice
    {
        public Guid? Id { get; set; }
        public byte Status { get; set; }
        public string Code { get; set; }
        public decimal Payment { get; set; }
        public decimal ToPay { get; set; }
        public byte PaymentStatus { get; set; }
        public byte PaymentMethod { get; set; }
        public byte Type { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public string? ClientLastName { get; set; }
        public string? CreatedByName { get; set; }
        public string? UpdatedByName { get; set; }
    }
}

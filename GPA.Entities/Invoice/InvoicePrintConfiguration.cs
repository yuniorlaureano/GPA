namespace GPA.Entities.Invoice
{
    public class InvoicePrintConfiguration
    {
        public Guid Id { get; set; }
        public string? CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDocument { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public bool Current { get; set; }
    }
}

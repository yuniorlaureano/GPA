using GPA.Entities.Invoice;

namespace GPA.Entities.Unmapped
{
    public class InvoicePrintData
    {
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDocument { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public string User { get; set; }
        public string Hour { get; set; }
        public string Date { get; set; }
        public Guid InvoiceId { get; set; }

        public List<InvoicePrintDetails> InvoicePrintDetails { get; set; } = new();

        public void SetParams(InvoicePrintConfiguration invoicePrintConfiguration)
        {
            CompanyLogo = invoicePrintConfiguration.CompanyLogo;
            CompanyName = invoicePrintConfiguration.CompanyName;
            CompanyDocument = invoicePrintConfiguration.CompanyDocument;
            CompanyAddress = invoicePrintConfiguration.CompanyAddress;
            CompanyPhone = invoicePrintConfiguration.CompanyPhone;
            CompanyEmail = invoicePrintConfiguration.CompanyEmail;
            CompanyWebsite = invoicePrintConfiguration.CompanyWebsite;
        }
    }
}

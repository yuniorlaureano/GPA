using GPA.Common.Entities.Invoice;
using GPA.Entities.Unmapped.General;
using GPA.Entities.Unmapped.Invoice;

namespace GPA.Entities.Unmapped
{
    public class InvoicePrintData
    {
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDocument { get; set; }
        public string CompanyDocumentPrefix { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyPhonePrefix { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public string User { get; set; }
        public string Hour { get; set; }
        public string Date { get; set; }
        public string Signer { get; set; }

        public RawClient Client { get; set; }
        public RawInvoice Invoice { get; set; }
        public List<InvoicePrintDetails> InvoicePrintDetails { get; set; } = new();
        public ClientPaymentsDetails ReceivableAccounts { get; set; } = new();

        public void SetParams(RawPrintInformation invoicePrintConfiguration)
        {
            CompanyLogo = invoicePrintConfiguration.CompanyLogo;
            CompanyName = invoicePrintConfiguration.CompanyName;
            CompanyDocument = invoicePrintConfiguration.CompanyDocument;
            CompanyDocumentPrefix = invoicePrintConfiguration.CompanyDocumentPrefix;
            CompanyAddress = invoicePrintConfiguration.CompanyAddress;
            CompanyPhone = invoicePrintConfiguration.CompanyPhone;
            CompanyPhonePrefix = invoicePrintConfiguration.CompanyPhonePrefix;
            CompanyEmail = invoicePrintConfiguration.CompanyEmail;
            CompanyWebsite = invoicePrintConfiguration.CompanyWebsite;
            Signer = invoicePrintConfiguration.Signer;
        }
    }
}

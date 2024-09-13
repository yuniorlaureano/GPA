namespace GPA.Entities.Unmapped.General
{
    public class RawPrintInformation
    {
        public Guid Id { get; set; }
        public string? CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDocument { get; set; }
        public string CompanyDocumentPrefix { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyPhonePrefix { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public string Signer { get; set; }
        public bool Current { get; set; }

        public Guid? StoreId { get; set; }
    }
}

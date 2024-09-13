namespace GPA.Dtos.General
{
    public class UpdatePrintInformationDto
    {
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

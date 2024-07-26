namespace GPA.Dtos.General
{
    public class EmailConfigurationDto
    {
        public Guid Id { get; set; }
        public string Vendor { get; set; }
        public string Value { get; set; }
        public string From { get; set; }
        public bool Current { get; set; }
    }
}

namespace GPA.Dtos.Network
{
    public class EmailConfigurationCreationDto
    {
        public string Vendor { get; set; }
        public string Value { get; set; }
        public string From { get; set; }
        public bool Current { get; set; }
    }
}

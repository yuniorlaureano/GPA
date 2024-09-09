namespace GPA.Entities.Unmapped.Audit
{
    public class ClientHistory : EntityHistory<Guid>
    {
        public string Name { get; set; }
        public string Identification { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? Address { get; set; }
        public string? FormattedAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string CreditsHistory { get; set; }
    }
}

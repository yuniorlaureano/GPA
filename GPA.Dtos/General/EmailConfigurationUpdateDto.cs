namespace GPA.Dtos.General
{
    public class EmailConfigurationUpdateDto
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; }
        public string Engine { get; set; }
        public string Value { get; set; }
        public string From { get; set; }
        public bool Current { get; set; }
    }
}

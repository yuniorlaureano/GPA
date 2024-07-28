namespace GPA.Entities.General
{
    public class EmailConfiguration : Entity<Guid>
    {
        public string Identifier { get; set; }
        public string Engine { get; set; }
        public string Value { get; set; }
        public string From { get; set; }
        public bool Current { get; set; }
    }
}

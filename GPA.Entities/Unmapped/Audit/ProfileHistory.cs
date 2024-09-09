namespace GPA.Entities.Unmapped.Audit
{
    public class ProfileHistory : EntityHistory<Guid>
    {
        public string Name { get; set; }
        public string? Value { get; set; }
    }
}

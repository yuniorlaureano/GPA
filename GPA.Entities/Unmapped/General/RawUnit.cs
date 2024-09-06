namespace GPA.Entities.Unmapped.General
{
    public class RawUnit
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Deleted { get; set; }
    }
}

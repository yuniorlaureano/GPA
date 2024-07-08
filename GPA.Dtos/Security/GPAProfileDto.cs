namespace GPA.Dtos.Security
{
    public class GPAProfileDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string? Permissions { get; set; }
    }
}

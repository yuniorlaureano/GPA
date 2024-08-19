namespace GPA.Entities.Unmapped
{
    public class RawUser
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string? Photo { get; set; }
        public string Email { get; set; }
        public bool Deleted { get; set; }
        public bool IsAssigned { get; set; }
    }
}

using GPA.Common.DTOs.Unmapped;

namespace GPA.Dtos.Security
{
    public class GPAUserDto
    {
        public Guid? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<RawProfileDto> Profiles { get; set; } = new();
    }
}

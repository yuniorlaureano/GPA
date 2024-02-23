using System.Security.Claims;

namespace GPA.Dtos.Security
{
    public class TokenDescriptorDto
    {
        public Claim[] Claims { get; set; }
        public string Algorithm { get; set; }
    }
}

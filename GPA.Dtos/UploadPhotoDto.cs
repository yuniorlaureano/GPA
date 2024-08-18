using Microsoft.AspNetCore.Http;

namespace GPA.Dtos
{
    public class UploadPhotoDto
    {
        public Guid Id { get; set; }
        public IFormFile Photo { get; set; }
    }
}

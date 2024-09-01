using Microsoft.AspNetCore.Http;

namespace GPA.Dtos.General
{
    public class PrintInformationUploadPhotoDto
    {
        public Guid PrintInformationId { get; set; }
        public IFormFile Photo { get; set; }
    }
}

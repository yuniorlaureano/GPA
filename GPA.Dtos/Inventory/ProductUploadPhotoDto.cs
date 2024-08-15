using Microsoft.AspNetCore.Http;

namespace GPA.Dtos.Inventory
{
    public class ProductUploadPhotoDto
    {
        public Guid ProductId { get; set; }
        public IFormFile Photo { get; set; }
    }
}

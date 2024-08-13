using Microsoft.AspNetCore.Http;

namespace GPA.Dtos.General
{
    public interface IBlobStorageService
    {
        public string Provider { get; }
        Task<BlobStorageFileResult> UploadFile(IFormFile file, string options, string folder = "");
        Task<Stream> DownloadFile(string options, string fileName);
        Task DeleteFile(string options, string fileName);
    }
}

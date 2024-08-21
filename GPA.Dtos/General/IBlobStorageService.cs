using Microsoft.AspNetCore.Http;

namespace GPA.Dtos.General
{
    public interface IBlobStorageService
    {
        public string Provider { get; }
        Task<BlobStorageFileResult> UploadFile(IFormFile file, string options, string folder = "", bool isPublic = false, string publicUrl = "");
        Task<Stream> DownloadFile(string options, string fileName, bool isPublic = false);
        Task DeleteFile(string options, string fileName, bool isPublic = false);
    }
}

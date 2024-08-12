using Microsoft.AspNetCore.Http;

namespace GPA.Dtos.General
{
    public interface IBlobStorageService
    {
        public string Provider { get; }
        Task<BlobStorageFileResult> UploadFile(IFormFile file, string options);
        Task DownloadFile(string localFilePath, string options);
        Task DeleteFile(string localFilePath, string options);
    }
}

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using GPA.Dtos.General;
using GPA.Utils;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace GPA.Services.General.BlobStorage
{
    public class GCPBucketService : IBlobStorageService
    {
        public string Provider => BlobStorageConstants.GCP;
        private readonly IBlobStorageHelper _blobStorageProviderHelper;
        private StorageClient _storageClient;
        private GCPBucketOptions _gCPBucketOptions;

        public GCPBucketService(IBlobStorageHelper blobStorageProviderHelper)
        {
            _blobStorageProviderHelper = blobStorageProviderHelper;
        }

        public async Task DeleteFile(string options, string fileName, string bucketOrContainer)
        {
            if (_storageClient is null)
            {
                await Configure(options);
            }

            await _storageClient.DeleteObjectAsync(bucketOrContainer, fileName);
        }

        public async Task<Stream> DownloadFile(string options, string fileName, string bucketOrContainer)
        {
            if (_storageClient is null)
            {
                await Configure(options);
            }

            var memoryStream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(bucketOrContainer, fileName, memoryStream);
            return memoryStream;
        }

        public async Task<BlobStorageFileResult> UploadFile(IFormFile file, string options, string folder = "", bool isPublic = false, string publicUrl = "")
        {
            if (_storageClient is null)
            {
                await Configure(options);
            }

            var fileResult = new BlobStorageFileResult();
            var filePath = Path.GetTempFileName();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                fileResult.FileName = file.FileName;
            }

            fileResult.UniqueFileName = $"{folder}{Guid.NewGuid()}-{file.FileName}";

            if (isPublic)
            {
                fileResult.FileUrl = $"{publicUrl}/{_gCPBucketOptions.PublicBucket}/{fileResult.UniqueFileName}";
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                var bucket = isPublic ? _gCPBucketOptions.PublicBucket : _gCPBucketOptions.PrivateBucket;
                await _storageClient.UploadObjectAsync(bucket, fileResult.UniqueFileName, file.ContentType, fileStream);
            }
            File.Delete(filePath);

            return fileResult;
        }

        public async Task Configure(string options)
        {
            var cancellationToken = new CancellationToken();
            _gCPBucketOptions = (GCPBucketOptions)_blobStorageProviderHelper.DecryptCredentialsInOptions(options, BlobStorageConstants.GCP);
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(_gCPBucketOptions.JsonCredentials));
            var credential = await GoogleCredential.FromStreamAsync(jsonStream, cancellationToken);

            _storageClient = StorageClient.Create(credential);
        }
    }
}

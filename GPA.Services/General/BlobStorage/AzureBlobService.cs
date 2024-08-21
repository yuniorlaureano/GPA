using Azure.Storage.Blobs;
using GPA.Dtos.General;
using GPA.Utils;
using Microsoft.AspNetCore.Http;

namespace GPA.Services.General.BlobStorage
{
    public class AzureBlobService : IBlobStorageService
    {
        public string Provider => BlobStorageConstants.AZURE;
        private readonly IBlobStorageHelper _blobStorageProviderHelper;
        private BlobServiceClient _blobServiceClient;
        private AzureBlobOptions _azureBlobOptions;

        public AzureBlobService(IBlobStorageHelper blobStorageProviderHelper)
        {
            _blobStorageProviderHelper = blobStorageProviderHelper;
        }

        public async Task DeleteFile(string options, string fileName, bool isPublic = false)
        {
            if (_blobServiceClient is null)
            {
                await Configure(options);
            }

            var bucketOrContainer = isPublic ? _azureBlobOptions.PublicContainer : _azureBlobOptions.PrivateContainer;
            var containerClient = _blobServiceClient.GetBlobContainerClient(bucketOrContainer);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<Stream> DownloadFile(string options, string fileName, bool isPublic = false)
        {
            if (_blobServiceClient is null)
            {
                await Configure(options);
            }

            var bucketOrContainer = isPublic ? _azureBlobOptions.PublicContainer : _azureBlobOptions.PrivateContainer;
            var containerClient = _blobServiceClient.GetBlobContainerClient(bucketOrContainer);
            var blobClient = containerClient.GetBlobClient(fileName);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            return memoryStream;
        }

        public async Task<BlobStorageFileResult> UploadFile(IFormFile file, string options, string folder = "", bool isPublic = false, string publicUrl = "")
        {
            if (_blobServiceClient is null)
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
                fileResult.FileUrl = $"{publicUrl}/{_azureBlobOptions.PublicContainer}/{fileResult.UniqueFileName}";
            }

            var container = isPublic ? _azureBlobOptions.PublicContainer : _azureBlobOptions.PrivateContainer;
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(fileResult.UniqueFileName);

            using (var fileStream = File.OpenRead(filePath))
            {
                await blobClient.UploadAsync(fileStream, true);
            }

            File.Delete(filePath);

            return fileResult;
        }

        public Task Configure(string options)
        {
            _azureBlobOptions = (AzureBlobOptions)_blobStorageProviderHelper.DecryptCredentialsInOptions(options, BlobStorageConstants.AZURE);
            _blobServiceClient = new BlobServiceClient(_azureBlobOptions.ConnectionString);
            return Task.FromResult(0);
        }
    }
}

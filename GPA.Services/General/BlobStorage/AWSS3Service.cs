using Amazon.S3;
using Amazon.S3.Transfer;
using GPA.Dtos.General;
using GPA.Utils;
using Microsoft.AspNetCore.Http;

namespace GPA.Services.General.BlobStorage
{
    public class AWSS3Service : IBlobStorageService
    {
        public string Provider => BlobStorageConstants.AWS;
        private readonly IBlobStorageHelper _blobStorageProviderHelper;
        private IAmazonS3 _s3Client;
        private AWSS3Options _aWSS3Options;

        public AWSS3Service(IBlobStorageHelper blobStorageProviderHelper)
        {
            _blobStorageProviderHelper = blobStorageProviderHelper;
        }

        public async Task DeleteFile(string options, string fileName, string bucketOrContainer)
        {
            if (_s3Client is null)
            {
                await Configure(options);
            }

            var fileTransferUtility = new TransferUtility(_s3Client);
            await _s3Client.DeleteObjectAsync(bucketOrContainer, fileName);
        }

        public async Task<Stream> DownloadFile(string options, string fileName, string bucketOrContainer)
        {
            if (_s3Client is null)
            {
                await Configure(options);
            }

            var filePath = Path.GetTempFileName();
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.DownloadAsync(filePath, bucketOrContainer, fileName);
            var memoryStream = new MemoryStream();
            using (var file = new FileStream(filePath, FileMode.Open)) 
            {
                file.CopyTo(memoryStream);
            }
            File.Delete(filePath);
            return memoryStream;
        }

        public async Task<BlobStorageFileResult> UploadFile(IFormFile file, string options, string folder = "", bool isPublic = false)
        {
            if (_s3Client is null)
            {
                await Configure(options);
            }

            var fileResult = new BlobStorageFileResult()
            {
                Provider = Provider
            };
            var filePath = Path.GetTempFileName();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                fileResult.FileName = file.FileName;
            }

            fileResult.UniqueFileName = $"{folder}{Guid.NewGuid()}-{file.FileName}";
            var fileTransferUtility = new TransferUtility(_s3Client);
            fileResult.BucketOrContainer = isPublic ? _aWSS3Options.PublicBucket : _aWSS3Options.PrivateBucket;
            await fileTransferUtility.UploadAsync(filePath, fileResult.BucketOrContainer, fileResult.UniqueFileName);
            File.Delete(filePath);

            return fileResult;
        }

        public Task Configure(string options)
        {
            _aWSS3Options = (AWSS3Options)_blobStorageProviderHelper.DecryptCredentialsInOptions(options, BlobStorageConstants.AWS);
            var config = new AmazonS3Config()
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_aWSS3Options.Region)
            };

            _s3Client = new AmazonS3Client(_aWSS3Options.AccessKeyId, _aWSS3Options.SecretAccessKey, config);
            return Task.FromResult(0);
        }
    }
}

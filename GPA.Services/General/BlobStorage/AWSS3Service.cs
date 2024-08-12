using Amazon.S3;
using Amazon.S3.Transfer;
using GPA.Dtos.General;
using GPA.Utils;
using Microsoft.AspNetCore.Http;

namespace GPA.Services.General.BlobStorage
{
    public class AWSS3Service : IBlobStorageService
    {
        public string Provider { get => BlobStorageConstants.AWS; }
        private readonly IBlobStorageHelper _blobStorageProviderHelper;
        private IAmazonS3 _s3Client;
        private AWSS3Options _aWSS3Options;

        public AWSS3Service(IBlobStorageHelper blobStorageProviderHelper)
        {
            _blobStorageProviderHelper = blobStorageProviderHelper;
        }

        public async Task DeleteFile(string localFilePath, string options)
        {
            if (_s3Client is null)
            {
                await Configure(options);
            }

            var keyName = $"[{Guid.NewGuid()}]";
            var fileTransferUtility = new TransferUtility(_s3Client);
            await _s3Client.DeleteObjectAsync(_aWSS3Options.Bucket, keyName);
        }

        public async Task DownloadFile(string localFilePath, string options)
        {
            if (_s3Client is null)
            {
                await Configure(options);
            }

            var keyName = $"[{Guid.NewGuid()}]";
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.DownloadAsync(localFilePath, _aWSS3Options.Bucket, keyName);
        }

        public async Task<BlobStorageFileResult> UploadFile(IFormFile file, string options)
        {
            if (_s3Client is null)
            {
                await Configure(options);
            }

            var fileResult = new BlobStorageFileResult();
            var filePath = Path.GetTempFileName();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                fileResult.FileName = file.FileName;
                fileResult.FileSize = file.Length;
            }

            fileResult.UniqueFileName = $"{Guid.NewGuid()}-{file.FileName}";
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(filePath, _aWSS3Options.Bucket, fileResult.UniqueFileName);
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

using GPA.Data.General;
using GPA.Dtos.General;
using Microsoft.AspNetCore.Http;

namespace GPA.Services.General.BlobStorage
{
    public interface IBlobStorageServiceFactory
    {
        Task<BlobStorageFileResult> UploadFile(IFormFile file, string folder = "", bool isPublic = false);
        Task<Stream?> DownloadFile(string fileName, string bucketOrContainer);
        Task DeleteFile(string fileName, string bucketOrContainer);
    }

    public class BlobStorageServiceFactory : IBlobStorageServiceFactory
    {
        private readonly IBlobStorageConfigurationRepository _blobStorageConfigurationRepository;
        private readonly IEnumerable<IBlobStorageService> _blobStorageServices;

        public BlobStorageServiceFactory(
            IBlobStorageConfigurationRepository blobStorageConfigurationRepository,
            IEnumerable<IBlobStorageService> blobStorageServices)
        {
            _blobStorageConfigurationRepository = blobStorageConfigurationRepository;
            _blobStorageServices = blobStorageServices;
        }

        public async Task<BlobStorageFileResult> UploadFile(IFormFile file, string folder = "", bool isPublic = false)
        {
            var config = await _blobStorageConfigurationRepository.GetByIdAsync(query => query, x => x.Current);
            if (config is null)
            {
                throw new ArgumentNullException("No existe configuración activa");
            }

            var blobService = _blobStorageServices.Where(x => x.Provider == config.Provider).FirstOrDefault();

            if (blobService is null)
            {
                throw new ArgumentException("No se ha encontrado el proveedor de archivos");
            }

            return await blobService.UploadFile(file, config.Value, folder, isPublic);
        }

        public async Task<Stream?> DownloadFile(string fileName, string bucketOrContainer)
        {
            var config = await _blobStorageConfigurationRepository.GetByIdAsync(query => query, x => x.Current);
            if (config is null)
            {
                throw new ArgumentNullException("No existe configuración activa");
            }

            var blobService = _blobStorageServices.Where(x => x.Provider == config.Provider).FirstOrDefault();

            if (blobService is null)
            {
                throw new ArgumentException("No se ha encontrado el proveedor de archivos");
            }

            var stream = await blobService.DownloadFile(config.Value, fileName, bucketOrContainer);
            if (stream is null)
            {
                return null;
            }

            stream.Position = 0;
            return stream;
        }

        public async Task DeleteFile(string fileName, string bucketOrContainer)
        {
            var config = await _blobStorageConfigurationRepository.GetByIdAsync(query => query, x => x.Current);
            if (config is null)
            {
                throw new ArgumentNullException("No existe configuración activa");
            }

            var blobService = _blobStorageServices.Where(x => x.Provider == config.Provider).FirstOrDefault();

            if (blobService is null)
            {
                throw new ArgumentException("No se ha encontrado el proveedor de archivos");
            }

            await blobService.DeleteFile(config.Value, fileName, bucketOrContainer);
        }
    }
}

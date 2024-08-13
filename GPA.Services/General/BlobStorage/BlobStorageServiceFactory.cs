using GPA.Data.General;
using GPA.Dtos.General;
using Microsoft.AspNetCore.Http;

namespace GPA.Services.General.BlobStorage
{
    public interface IBlobStorageServiceFactory
    {
        Task<BlobStorageFileResult> UploadFile(IFormFile file);
        Task<Stream?> DownloadFile(string fileName);
        Task DeleteFile(string fileName);
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

        public async Task<BlobStorageFileResult> UploadFile(IFormFile file)
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

            return await blobService.UploadFile(file, config.Value);
        }

        public async Task<Stream?> DownloadFile(string fileName)
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

            var stream = await blobService.DownloadFile(config.Value, fileName);
            if (stream is null)
            {
                return null;
            }

            stream.Position = 0;
            return stream;
        }

        public async Task DeleteFile(string fileName)
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

            await blobService.DeleteFile(config.Value, fileName);
        }
    }
}

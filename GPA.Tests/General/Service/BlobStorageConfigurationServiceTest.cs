using AutoFixture;
using GPA.Dtos.General;
using GPA.Services.General;
using GPA.Tests.Fixtures;
using GPA.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace GPA.Tests.General.Service
{
    public class BlobStorageConfigurationServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IBlobStorageConfigurationService _blobStorageConfigurationService;

        public BlobStorageConfigurationServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _blobStorageConfigurationService = _services.GetRequiredService<IBlobStorageConfigurationService>();
        }

    }
}
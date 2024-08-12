using AutoFixture;
using GPA.Dtos.General;
using GPA.Entities.General;
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



        [Fact]
        public async Task ShouldOneCreate()
        {
            var options = new AWSS3Options
            {
                AccessKeyId = "",
                SecretAccessKey = "",
                Bucket = "gpa-file-storage",
                Region = "us-east-2",
            };

            var configuration = _fixture
                .Build<BlobStorageConfigurationCreationDto>()
                .With(x => x.Identifier, "AWS")
                .With(x => x.Provider, BlobStorageConstants.AWS)
                .With(x => x.Value, JsonSerializer.Serialize(options, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }))
                .With(x => x.Current, true)
                .Create();

            await _blobStorageConfigurationService.CreateConfigurationAsync(configuration);
            Assert.True(true);
        }
    }
}
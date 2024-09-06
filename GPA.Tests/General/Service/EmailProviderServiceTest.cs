using AutoFixture;
using GPA.Dtos.General;
using GPA.Services.General;
using GPA.Tests.Fixtures;
using GPA.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace GPA.Tests.General.Service
{
    public class EmailProviderServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IEmailProviderService _iEmailProviderService;

        public EmailProviderServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _iEmailProviderService = _services.GetRequiredService<IEmailProviderService>();
        }

        
    }
}
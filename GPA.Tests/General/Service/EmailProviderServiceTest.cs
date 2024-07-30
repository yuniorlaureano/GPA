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



        [Fact]
        public async Task ShouldOneCreate()
        {
            var smtpOption = new SmtpEmailOptions
            {
                Host = "smtp.gmail.com",
                UserName = "2018-0025@unad.edu.do",
                Password = "gikg idwq nvii fbrh",
                Port = 587,
                UseSsl = true
            };

            var configuration = _fixture
                .Build<EmailConfigurationCreationDto>()
                .With(x => x.Identifier, "Google")
                .With(x => x.Engine, EmailConstants.SMTP)
                .With(x => x.Value, JsonSerializer.Serialize(smtpOption, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }))
                .With(x => x.From, "2018-0025@unad.edu.do")
                .With(x => x.Current, true)
                .Create();

            await _iEmailProviderService.CreateConfigurationAsync(configuration);
            Assert.True(true);
        }
    }
}
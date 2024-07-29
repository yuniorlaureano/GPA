using GPA.Dtos.General;
using GPA.Utils;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace GPA.Services.General.Email
{
    public class SendGridEmailService : IEmailService
    {
        public string Engine => EmailConstants.SENGRID;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IHttpClientFactory httpClientFactory, ILogger<SendGridEmailService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        public async Task SendEmail(IGPAEmailMessage mailMessage, string options)
        {
            try
            {
                var mgs = (SendGridEmailMessage)mailMessage;
                var client = Configure(options);
                var content = await client.PostAsJsonAsync("mail/send", mgs.GetMessage());
                content.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email message for provider: {Provider}, {Message}", Engine, ex.Message);
                throw;
            }
        }

        private HttpClient Configure(string options)
        {
            var sendGridOptions = JsonSerializer.Deserialize<SendGridEmailOptions>(options, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var client = _httpClientFactory.CreateClient(UrlConstant.SENDGRID);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {sendGridOptions.Apikey}");
            return client;
        }
    }
}

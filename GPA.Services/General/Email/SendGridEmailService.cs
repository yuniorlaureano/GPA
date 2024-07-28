using GPA.Dtos.General;
using GPA.Utils;
using System.Net.Http.Json;
using System.Text.Json;

namespace GPA.Services.General.Email
{
    public class SendGridEmailService : IEmailService
    {
        public string Engine => EmailConstants.SENGRID;
        private readonly IHttpClientFactory _httpClientFactory;

        public SendGridEmailService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

using GPA.Dtos.General;
using GPA.Services.General.Security;
using System.Text.Json;

namespace GPA.Utils
{
    public interface IEmailProviderHelper
    {
        IEmailOptions EncryptCredentialsInOptions(string options, string engine, bool credentialChanged = true);
        IEmailOptions DecryptCredentialsInOptions(string options, string engine);
        string SerializeOptions(IEmailOptions options, string engine);
        bool CredentialChanged(string newOptions, string savedOptions, string engine);
    }

    public class EmailProviderHelper : IEmailProviderHelper
    {
        private readonly IAesHelper _aesHelper;
        private readonly JsonSerializerOptions serializerOptions;

        public EmailProviderHelper(IAesHelper aesHelper)
        {
            _aesHelper = aesHelper;
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public IEmailOptions? EncryptCredentialsInOptions(string options, string engine, bool credentialChanged = true)
        {
            return engine switch
            {
                EmailConstants.SMTP => EncryptSmtp(options, engine, credentialChanged),
                EmailConstants.SENGRID => EncryptSendGrid(options, engine, credentialChanged),
                _ => null
            };
        }

        public IEmailOptions? DecryptCredentialsInOptions(string options, string engine)
        {
            return engine switch
            {
                EmailConstants.SMTP => DecryptSmtp(options, engine),
                EmailConstants.SENGRID => DecryptSendGrid(options, engine),
                _ => null
            };
        }

        public IEmailOptions? EncryptSmtp(string options, string engine, bool credentialChanged)
        {
            var optionsObject = JsonSerializer.Deserialize<SmtpEmailOptions>(options, serializerOptions);
            if (credentialChanged)
            {
                optionsObject.Password = _aesHelper.Encrypt(optionsObject.Password);
            }           
            return optionsObject;
        }

        public IEmailOptions? EncryptSendGrid(string options, string engine, bool credentialChanged)
        {
            var optionsObject = JsonSerializer.Deserialize<SendGridEmailOptions>(options, serializerOptions);
            if (credentialChanged)
            {
                optionsObject.Apikey = _aesHelper.Encrypt(optionsObject.Apikey);
            }            
            return optionsObject;
        }

        public IEmailOptions? DecryptSmtp(string options, string engine)
        {
            var optionsObject = JsonSerializer.Deserialize<SmtpEmailOptions>(options, serializerOptions);
            optionsObject.Password = _aesHelper.Decrypt(optionsObject.Password);
            return optionsObject;
        }

        public IEmailOptions? DecryptSendGrid(string options, string engine)
        {
            var optionsObject = JsonSerializer.Deserialize<SendGridEmailOptions>(options, serializerOptions);
            optionsObject.Apikey = _aesHelper.Decrypt(optionsObject.Apikey);
            return optionsObject;
        }

        public string SerializeOptions(IEmailOptions options, string engine)
        {
            return engine switch
            {
                EmailConstants.SMTP => JsonSerializer.Serialize((SmtpEmailOptions)options, serializerOptions),
                EmailConstants.SENGRID => JsonSerializer.Serialize((SendGridEmailOptions)options, serializerOptions),
                _ => string.Empty
            };
        }

        public bool CredentialChanged(string newOptions, string savedOptions, string engine)
        {
            return engine switch
            {
                EmailConstants.SMTP => SmtpCredentialsChanged(newOptions, savedOptions),
                EmailConstants.SENGRID => SendGridCredentialsChanged(newOptions, savedOptions),
                _ => false
            };
        }

        private bool SmtpCredentialsChanged(string newOptions, string savedOptions)
        { 
            var newOptionsObject = JsonSerializer.Deserialize<SmtpEmailOptions>(newOptions, serializerOptions);
            var savedOptionsObject = JsonSerializer.Deserialize<SmtpEmailOptions>(savedOptions, serializerOptions);

            return newOptionsObject.Password != savedOptionsObject.Password;
        }

        private bool SendGridCredentialsChanged(string newOptions, string savedOptions)
        {
            var newOptionsObject = JsonSerializer.Deserialize<SendGridEmailOptions>(newOptions, serializerOptions);
            var savedOptionsObject = JsonSerializer.Deserialize<SendGridEmailOptions>(savedOptions, serializerOptions);

            return newOptionsObject.Apikey != savedOptionsObject.Apikey;
        }
    }
}

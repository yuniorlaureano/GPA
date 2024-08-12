using GPA.Dtos.General;
using GPA.Services.General.Security;
using System.Text.Json;

namespace GPA.Utils
{
    public interface IBlobStorageHelper
    {
        IGPABlobStorageOptions EncryptCredentialsInOptions(string options, string provider, bool credentialChanged = true);
        IGPABlobStorageOptions DecryptCredentialsInOptions(string options, string provider);
        string SerializeOptions(IGPABlobStorageOptions options, string provider);
        bool CredentialChanged(string newOptions, string savedOptions, string provider);
    }

    public class BlobStorageHelper : IBlobStorageHelper
    {
        private readonly IAesHelper _aesHelper;
        private readonly JsonSerializerOptions serializerOptions;

        public BlobStorageHelper(IAesHelper aesHelper)
        {
            _aesHelper = aesHelper;
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public IGPABlobStorageOptions? EncryptCredentialsInOptions(string options, string provider, bool credentialChanged = true)
        {
            return provider switch
            {
                BlobStorageConstants.AWS => EncryptAWS(options, provider, credentialChanged),
                _ => null
            };
        }

        public IGPABlobStorageOptions? DecryptCredentialsInOptions(string options, string provider)
        {
            return provider switch
            {
                BlobStorageConstants.AWS => DecryptAWS(options, provider),
                _ => null
            };
        }

        public IGPABlobStorageOptions? EncryptAWS(string options, string provider, bool credentialChanged)
        {
            var optionsObject = JsonSerializer.Deserialize<AWSS3Options>(options, serializerOptions);
            if (credentialChanged)
            {
                optionsObject.AccessKeyId = _aesHelper.Encrypt(optionsObject.AccessKeyId);
                optionsObject.SecretAccessKey = _aesHelper.Encrypt(optionsObject.SecretAccessKey);
            }            
            return optionsObject;
        }

        public IGPABlobStorageOptions? DecryptAWS(string options, string provider)
        {
            var optionsObject = JsonSerializer.Deserialize<AWSS3Options>(options, serializerOptions);
            optionsObject.AccessKeyId = _aesHelper.Decrypt(optionsObject.AccessKeyId);
            optionsObject.SecretAccessKey = _aesHelper.Decrypt(optionsObject.SecretAccessKey);
            return optionsObject;
        }

        public string SerializeOptions(IGPABlobStorageOptions options, string provider)
        {
            return provider switch
            {
                BlobStorageConstants.AWS => JsonSerializer.Serialize((AWSS3Options)options, serializerOptions),
                _ => string.Empty
            };
        }

        public bool CredentialChanged(string newOptions, string savedOptions, string provider)
        {
            return provider switch
            {
                BlobStorageConstants.AWS => ASWCredentialsChanged(newOptions, savedOptions),
                _ => false
            };
        }

        private bool ASWCredentialsChanged(string newOptions, string savedOptions)
        { 
            var newOptionsObject = JsonSerializer.Deserialize<AWSS3Options>(newOptions, serializerOptions);
            var savedOptionsObject = JsonSerializer.Deserialize<AWSS3Options>(savedOptions, serializerOptions);

            return newOptionsObject.AccessKeyId != savedOptionsObject.AccessKeyId ||
                   newOptionsObject.SecretAccessKey != savedOptionsObject.SecretAccessKey;
        }
    }
}

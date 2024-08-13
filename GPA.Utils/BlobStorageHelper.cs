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

        #region Encrypt
        public IGPABlobStorageOptions? EncryptCredentialsInOptions(string options, string provider, bool credentialChanged = true)
        {
            return provider switch
            {
                BlobStorageConstants.AWS => EncryptAWS(options, credentialChanged),
                BlobStorageConstants.GCP => EncryptGCP(options, credentialChanged),
                BlobStorageConstants.AZURE => EncryptAzure(options, credentialChanged),
                _ => null
            };
        }

        public IGPABlobStorageOptions? EncryptAWS(string options, bool credentialChanged)
        {
            var optionsObject = JsonSerializer.Deserialize<AWSS3Options>(options, serializerOptions);
            if (credentialChanged)
            {
                optionsObject.AccessKeyId = _aesHelper.Encrypt(optionsObject.AccessKeyId);
                optionsObject.SecretAccessKey = _aesHelper.Encrypt(optionsObject.SecretAccessKey);
            }            
            return optionsObject;
        }

        public IGPABlobStorageOptions? EncryptGCP(string options, bool credentialChanged)
        {
            var optionsObject = JsonSerializer.Deserialize<GCPBucketOptions>(options, serializerOptions);
            if (credentialChanged)
            {
                optionsObject.JsonCredentials = _aesHelper.Encrypt(optionsObject.JsonCredentials);
            }
            return optionsObject;
        }

        public IGPABlobStorageOptions? EncryptAzure(string options, bool credentialChanged)
        {
            var optionsObject = JsonSerializer.Deserialize<AzureBlobOptions>(options, serializerOptions);
            if (credentialChanged)
            {
                optionsObject.ConnectionString = _aesHelper.Encrypt(optionsObject.ConnectionString);
            }
            return optionsObject;
        }
        #endregion

        #region Decrypt
        public IGPABlobStorageOptions? DecryptCredentialsInOptions(string options, string provider)
        {
            return provider switch
            {
                BlobStorageConstants.AWS => DecryptAWS(options),
                BlobStorageConstants.GCP => DecryptGCP(options),
                BlobStorageConstants.AZURE => DecryptAzure(options),
                _ => null
            };
        }

        public IGPABlobStorageOptions? DecryptAWS(string options)
        {
            var optionsObject = JsonSerializer.Deserialize<AWSS3Options>(options, serializerOptions);
            optionsObject.AccessKeyId = _aesHelper.Decrypt(optionsObject.AccessKeyId);
            optionsObject.SecretAccessKey = _aesHelper.Decrypt(optionsObject.SecretAccessKey);
            return optionsObject;
        }

        public IGPABlobStorageOptions? DecryptGCP(string options)
        {
            var optionsObject = JsonSerializer.Deserialize<GCPBucketOptions>(options, serializerOptions);
            optionsObject.JsonCredentials = _aesHelper.Decrypt(optionsObject.JsonCredentials);
            return optionsObject;
        }

        public IGPABlobStorageOptions? DecryptAzure(string options)
        {
            var optionsObject = JsonSerializer.Deserialize<AzureBlobOptions>(options, serializerOptions);
            optionsObject.ConnectionString = _aesHelper.Decrypt(optionsObject.ConnectionString);
            return optionsObject;
        }
        #endregion

        #region Desialize
        public string SerializeOptions(IGPABlobStorageOptions options, string provider)
        {
            return provider switch
            {
                BlobStorageConstants.AWS => JsonSerializer.Serialize((AWSS3Options)options, serializerOptions),
                BlobStorageConstants.GCP => JsonSerializer.Serialize((GCPBucketOptions)options, serializerOptions),
                BlobStorageConstants.AZURE => JsonSerializer.Serialize((AzureBlobOptions)options, serializerOptions),
                _ => string.Empty
            };
        }
        #endregion

        #region CredentialChanged
        public bool CredentialChanged(string newOptions, string savedOptions, string provider)
        {
            return provider switch
            {
                BlobStorageConstants.AWS => ASWCredentialsChanged(newOptions, savedOptions),
                BlobStorageConstants.GCP => GCPCredentialsChanged(newOptions, savedOptions),
                BlobStorageConstants.AZURE => AzureCredentialsChanged(newOptions, savedOptions),
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

        private bool GCPCredentialsChanged(string newOptions, string savedOptions)
        {
            var newOptionsObject = JsonSerializer.Deserialize<GCPBucketOptions>(newOptions, serializerOptions);
            var savedOptionsObject = JsonSerializer.Deserialize<GCPBucketOptions>(savedOptions, serializerOptions);

            return newOptionsObject.JsonCredentials != savedOptionsObject.JsonCredentials;
        }

        private bool AzureCredentialsChanged(string newOptions, string savedOptions)
        {
            var newOptionsObject = JsonSerializer.Deserialize<AzureBlobOptions>(newOptions, serializerOptions);
            var savedOptionsObject = JsonSerializer.Deserialize<AzureBlobOptions>(savedOptions, serializerOptions);

            return newOptionsObject.ConnectionString != savedOptionsObject.ConnectionString;
        }
        #endregion
    }
}

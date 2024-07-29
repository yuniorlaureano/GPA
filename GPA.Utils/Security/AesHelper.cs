using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace GPA.Services.General.Security
{
    public interface IAesHelper
    {
        string Encrypt(string plainText);
        string Decrypt(string data);
    }

    public class AesHelper : IAesHelper
    {
        private readonly string IV = string.Empty;
        private readonly string Key = string.Empty;

        public AesHelper(IConfiguration configuration)
        {
            IV = configuration["Aes:IV"] ?? throw new InvalidOperationException("Need to provide IV for Aes");
            Key = configuration["Aes:Key"] ?? throw new InvalidOperationException("Need to provide key for Aes");
        }

        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string data)
        {
            var encryptedData = Convert.FromBase64String(data);
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(
                    Encoding.UTF8.GetBytes(Key), Encoding.UTF8.GetBytes(IV));

                using (MemoryStream ms = new MemoryStream(encryptedData))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}

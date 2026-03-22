using System.Security.Cryptography;
using System.Text;
using EHRS.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EHRS.Infrastructure.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(IConfiguration configuration)
        {
            var keyString = configuration["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption key not found in configuration.");

            _key = Convert.FromBase64String(keyString);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = _key;

                // 1. استخراج الـ IV من أول 16 byte في البيانات المشفرة
                var iv = new byte[aes.BlockSize / 8]; // الـ IV طوله 16 byte في AES
                var cipher = new byte[fullCipher.Length - iv.Length];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                // 2. عملية فك التشفير باستخدام Stream
                using var ms = new MemoryStream(cipher);
                using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
            catch
            {
                // لو النص مش Base64 أو حصل غلط في المفتاح (بيانات قديمة) رجعها زي ما هي
                return cipherText;
            }
        }
    }
}
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Banking_Application
{
    public static class CryptoHelper
    {
        private static readonly string cryptoKeyName = "SSD_Banking_Key";
        private static readonly CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;

        public static string Encrypt(string plainText)
        {
            byte[] ivBytes = null;

            try
            {
                // Ensure key exists in storage
                if (!CngKey.Exists(cryptoKeyName, keyStorageProvider))
                {
                    var keyCreationParameters = new CngKeyCreationParameters
                    {
                        Provider = keyStorageProvider
                    };
                    CngKey.Create(new CngAlgorithm("AES"), cryptoKeyName, keyCreationParameters);
                }

                using Aes aes = new AesCng(cryptoKeyName, keyStorageProvider);
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();
                ivBytes = aes.IV;

                using MemoryStream ms = new();
                ms.Write(ivBytes, 0, ivBytes.Length); // prepend IV

                using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
                using (StreamWriter sw = new(cs))
                {
                    sw.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
            finally
            {
                ClearBytes(ivBytes);
            }
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText) || encryptedText.Length < 24)
                return encryptedText; // Probably not encrypted

            byte[] ivBytes = new byte[16];

            try
            {
                byte[] fullCipher = Convert.FromBase64String(encryptedText);
                if (fullCipher.Length < 17)
                    return encryptedText; // Not long enough to contain IV + cipher

                Array.Copy(fullCipher, 0, ivBytes, 0, ivBytes.Length);

                // Ensure key exists in storage
                if (!CngKey.Exists(cryptoKeyName, keyStorageProvider))
                {
                    var keyCreationParameters = new CngKeyCreationParameters
                    {
                        Provider = keyStorageProvider
                    };
                    CngKey.Create(new CngAlgorithm("AES"), cryptoKeyName, keyCreationParameters);
                }

                using Aes aes = new AesCng(cryptoKeyName, keyStorageProvider);
                aes.IV = ivBytes;

                using MemoryStream ms = new(fullCipher, ivBytes.Length, fullCipher.Length - ivBytes.Length);
                using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using StreamReader sr = new(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return encryptedText;
            }
            finally
            {
                ClearBytes(ivBytes);
            }
        }

        private static void ClearBytes(byte[] data)
        {
            if (data == null) return;
            for (int i = 0; i < data.Length; i++) data[i] = 0;
        }

        public static string CalculateHMAC(string input)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes("YourSuperSecretKey123!")))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash);
            }
        }

    }
}

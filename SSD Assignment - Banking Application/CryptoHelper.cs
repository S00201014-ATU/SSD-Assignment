using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Banking_Application
{
    public static class CryptoHelper
    {
        private static readonly string encryptionKey = "87vG4iWZp5LR0YnBj2XM0nUuF9qhSe1T";

        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
            aes.GenerateIV();
            using MemoryStream ms = new();
            ms.Write(aes.IV, 0, aes.IV.Length); // prepend IV
            using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText) || encryptedText.Length < 24)
            {
                // Probably not encrypted, just return as-is
                return encryptedText;
            }

            try
            {
                byte[] fullCipher = Convert.FromBase64String(encryptedText);
                if (fullCipher.Length < 17)
                    return encryptedText; // Not long enough to contain IV + cipher

                using Aes aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
                byte[] iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using MemoryStream ms = new(fullCipher, iv.Length, fullCipher.Length - iv.Length);
                using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using StreamReader sr = new(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return encryptedText; // If decryption fails, just return the original
            }
        }

    }
}

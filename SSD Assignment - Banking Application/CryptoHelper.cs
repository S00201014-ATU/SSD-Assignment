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
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] ivBytes = null;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = keyBytes;
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
                ClearBytes(keyBytes);
                ClearBytes(ivBytes);
            }
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText) || encryptedText.Length < 24)
                return encryptedText; // Probably not encrypted

            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] ivBytes = new byte[16];

            try
            {
                byte[] fullCipher = Convert.FromBase64String(encryptedText);
                if (fullCipher.Length < 17)
                    return encryptedText; // Not long enough to contain IV + cipher

                Array.Copy(fullCipher, 0, ivBytes, 0, ivBytes.Length);

                using Aes aes = Aes.Create();
                aes.Key = keyBytes;
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
                ClearBytes(keyBytes);
                ClearBytes(ivBytes);
            }
        }

        private static void ClearBytes(byte[] data)
        {
            if (data == null) return;
            for (int i = 0; i < data.Length; i++) data[i] = 0;
        }
    }
}

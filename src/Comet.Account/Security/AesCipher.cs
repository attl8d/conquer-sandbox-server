using System;
using System.IO;
using System.Security.Cryptography;

namespace Comet.Account.Security
{
    public static class AesCipher
    {
        public static string Encrypt(byte[] key, string message)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(message);
            }

            array = memoryStream.ToArray();
            return Convert.ToBase64String(array);
        }

        public static string Decrypt(byte[] key, string message)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(message);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new(buffer);
                using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
                using StreamReader streamReader = new(cryptoStream);
                return streamReader.ReadToEnd();
            }
        }
    }
}

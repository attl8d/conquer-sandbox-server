using System;
using System.IO;
using System.Security.Cryptography;

namespace Comet.Network.Security
{
    public sealed class DatabaseCipher
    {
        private byte[] mKey;

        public DatabaseCipher(byte[] key)
        {
            mKey = new byte[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                mKey[i] = key[i];
            }
        }

        public string Encrypt(string data)
        {
            using Aes aes = Aes.Create();
            aes.Key = mKey;
            aes.IV = new byte[16];

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new ();
            using CryptoStream cs = new (ms, encryptor, CryptoStreamMode.Write);
            using StreamWriter sw = new (cs);
            sw.Write(data);
            byte[] result = ms.ToArray();
            return Convert.ToBase64String(result);
        }

        public string Decrypt(string data)
        {
            byte[] buffer = Convert.FromBase64String(data);
            using Aes aes = Aes.Create();
            aes.Key = mKey;
            aes.IV = new byte[16];

            ICryptoTransform decryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new (buffer);
            using CryptoStream cs = new (ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new (cs);
            return sr.ReadToEnd();
        }
    }
}

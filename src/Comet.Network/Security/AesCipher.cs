using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Comet.Network.Security
{
    public sealed class AesCipher : ICipher
    {
        public static byte[] SharedKey => Encoding.ASCII.GetBytes("8y/B?E(H+MbQeThWmZq3t6w9z$C&F)J@");
        public static byte[] SharedIv => Encoding.ASCII.GetBytes("t6w9z$C&F)J@NcRf");

        private Aes mAes;
        private ICryptoTransform mEncryptor;
        private ICryptoTransform mDecryptor;

        private AesCipher()
        {
        }

        public static AesCipher Create()
        {
            var cipher = new AesCipher
            {
                mAes = Aes.Create()
            };

            cipher.mAes.Mode = CipherMode.CFB;
            cipher.mAes.KeySize = 256;
            cipher.mAes.BlockSize = 128;
            cipher.mAes.FeedbackSize = 8;
            cipher.mAes.Padding = PaddingMode.None;

            byte[] key = new byte[SharedKey.Length];
            byte[] iv = new byte[SharedIv.Length];
            SharedKey.CopyTo(key, 0);
            SharedIv.CopyTo(iv, 0);

            cipher.mAes.Key = key;
            cipher.mAes.IV = iv;

            cipher.mEncryptor = cipher.mAes.CreateEncryptor(cipher.mAes.Key, cipher.mAes.IV);
            cipher.mDecryptor = cipher.mAes.CreateDecryptor(cipher.mAes.Key, cipher.mAes.IV);
            return cipher;
        }

        /// <inheritdoc />
        public void GenerateKeys(object[] seeds)
        {
            byte[] key = (byte[])seeds[0];
            key.CopyTo(new Memory<byte>(mAes.Key));

            if (seeds.Length > 1)
            {
                byte[] iv = (byte[])seeds[1];
                iv.CopyTo(new Memory<byte>(mAes.IV));
            }
            else
            {
                byte[] iv = new byte[SharedIv.Length];
                SharedIv.CopyTo(iv, 0);
                mAes.IV = iv;
            }

            mEncryptor = mAes.CreateEncryptor(mAes.Key, mAes.IV);
            mDecryptor = mAes.CreateDecryptor(mAes.Key, mAes.IV);
        }

        /// <inheritdoc />
        public void Decrypt(Span<byte> src, Span<byte> dst)
        {
            using MemoryStream memory = new();
            using CryptoStream stream = new(memory, mDecryptor, CryptoStreamMode.Write);
            stream.Write(src);
            stream.FlushFinalBlock();

            byte[] decrypted = memory.ToArray();
            for (int i = 0; i < decrypted.Length; i++)
            {
                dst[i] = decrypted[i];
            }
        }

        /// <inheritdoc />
        public void Encrypt(Span<byte> src, Span<byte> dst)
        {
            using var memory = new MemoryStream();
            using var stream = new CryptoStream(memory, mEncryptor, CryptoStreamMode.Write);
            stream.Write(src);
            stream.FlushFinalBlock();

            byte[] encrypted = memory.ToArray();
            for (int i = 0; i < encrypted.Length; i++)
            {
                dst[i] = encrypted[i];
            }
        }
    }
}
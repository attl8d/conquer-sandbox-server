using System;
using System.Text;
using Comet.Network.Security.BouncyCastle;

namespace Comet.Network.Security
{
    public class Cast5Cipher : ICipher
    {
        private byte[] mEncIvec;
        private byte[] mDecIvec;
        private int mEncNum;
        private int mDecNum;

        private Cast5Engine mEngine;

        private const string DefaultSeed = "C238xs65pjy7HU9Q";

        public Cast5Cipher(string seed = DefaultSeed)
        {
            mEngine = new Cast5Engine();

            mEncIvec = new byte[8];
            mDecIvec = new byte[8];
            mEncNum = 0;
            mDecNum = 0;

            GenerateKeys(new object[] { Encoding.ASCII.GetBytes(seed) });
        }

        public void Decrypt(Span<byte> src, Span<byte> dst)
        {
            int length = src.Length;
            byte c, cc;
            for (int l = length, n = mDecNum, inc = 0, outc = 0; l > 0; l--)
            {
                if (n == 0)
                {
                    mEngine.EncryptBlock(mDecIvec, 0, mDecIvec, 0);
                }
                cc = src[inc++];
                c = mDecIvec[n];
                mDecIvec[n] = cc;
                dst[outc] = (byte)((c ^ cc) & 0xff);
                outc++;
                n = (n + 1) & 0x07;
                mDecNum = n;
            }
        }

        public void Encrypt(Span<byte> src, Span<byte> dst)
        {
            int length = src.Length;
            byte c;
            for (int l = length, n = mEncNum, inc = 0, outc = 0; l > 0; l--)
            {
                if (n == 0)
                {
                    mEngine.EncryptBlock(mEncIvec, 0, mEncIvec, 0);
                }
                c = (byte)((src[inc++] ^ mEncIvec[n]) & 0xff);
                dst[outc++] = c;
                mEncIvec[n] = c;
                n = (n + 1) & 0x07;
                mEncNum = n;
            }
        }

        public void GenerateKeys(object[] seeds)
        {
            byte[] encIV = new byte[8];
            byte[] decIV = new byte[8];
            byte[] key = new byte[16];

            for (int i = 0; i < Math.Min(16, (seeds[0] as byte[]).Length); i++)
            {
                key[i] = (seeds[0] as byte[])[i];
            }

            if (seeds.Length > 2)
            {
                encIV = (byte[])seeds[1];
                decIV = (byte[])seeds[2];
            }

            mEngine.SetKey(key);

            mEncIvec = encIV;
            mDecIvec = decIV;
            mEncNum = 0;
            mDecNum = 0;
        }
    }
}

using System;
using System.Threading.Tasks;
using Comet.Launcher.Server.States;
using Comet.Network.Packets.Updater;
using Org.BouncyCastle.Math;

namespace Comet.Launcher.Server.Packets
{
    internal sealed class MsgUpdHandshake : MsgUpdHandshake<Client>
    {
        public MsgUpdHandshake()
        {
            
        }

        /// <inheritdoc />
        public MsgUpdHandshake(BigInteger publicKey, BigInteger modulus)
            : base(publicKey, modulus)
        {
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(Client client)
        {
            if (!client.DiffieHellman.Initialize(PublicKey, Modulus))
            {
                throw new Exception("Couldn't initialize Diffie-Hellman!!!");
            }
            
            await client.SendAsync(new MsgUpdHandshake(client.DiffieHellman.PublicKey, client.DiffieHellman.Modulus));

            client.Cipher.GenerateKeys(new object[]
            {
                client.DiffieHellman.SharedKey.ToByteArrayUnsigned()
            });
        }
    }
}

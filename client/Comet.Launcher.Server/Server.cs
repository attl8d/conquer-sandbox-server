using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Comet.Launcher.Server.Packets;
using Comet.Launcher.Server.States;
using Comet.Network.Packets;
using Comet.Network.Sockets;
using Comet.Shared;

namespace Comet.Launcher.Server
{
    internal sealed class Server : TcpServerListener<Client>
    {
        private readonly PacketProcessor<Client> Processor;

        public Server() : base(100)
        {
            Processor = new PacketProcessor<Client>(ProcessAsync);
            Processor.StartAsync(CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override Task<Client> AcceptedAsync(Socket socket, Memory<byte> buffer)
        {
            return null;
        }

        protected override void Received(Client actor, ReadOnlySpan<byte> packet)
        {
            Processor.Queue(actor, packet.ToArray());
        }

        private async Task ProcessAsync(Client actor, byte[] packet)
        {
            // Validate connection
            if (!actor.Socket.Connected)
                return;
            
            var length = BitConverter.ToUInt16(packet, 0);
            var type = (PacketType)BitConverter.ToUInt16(packet, 2);

            try
            {
                // Switch on the packet type
                MsgBase<Client> msg = null;
                switch (type)
                {
                    case PacketType.MsgUpdLogin:
                    {
                        msg = new MsgUpdLogin();
                        break;
                    }


                    default:
                        await Log.WriteLogAsync(LogLevel.Socket, "Missing packet {0}, Length {1}\n{2}",
                                          type, length, PacketDump.Hex(packet));
                        return;
                }

                // Decode packet bytes into the structure and process
                msg.Decode(packet);
                await msg.ProcessAsync(actor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void Disconnected(Client actor)
        {
        }
    }
}

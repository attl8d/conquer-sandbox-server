using System.Net.Sockets;
using Comet.Launcher.Packets;
using Comet.Launcher.States;
using Comet.Launcher.Threads;
using Comet.Network.Packets;
using Comet.Network.Sockets;
using Comet.Shared;

namespace Comet.Launcher
{
    public sealed class Client : TcpClientWrapper<Server>
    {
        private readonly LauncherThread mThread;
        private readonly PacketProcessor<Server> mProcessor;

        public Client(LauncherThread thread)
            : base("YybU6ItTFX".Length)
        {
            mThread = thread;
            mProcessor = new PacketProcessor<Server>(ProcessAsync, 1);
            mProcessor.StartAsync(CancellationToken.None).ConfigureAwait(false);
        }

        protected override async Task<Server> ConnectedAsync(Socket socket, Memory<byte> buffer)
        {
            Server client = new(socket, buffer, mProcessor.SelectPartition());
            if (socket.Connected)
            {
                await mThread.OnConnectAsync(client);
                return client;
            }
            return null;
        }

        protected override void Received(Server actor, ReadOnlySpan<byte> packet)
        {
            mProcessor.Queue(actor, packet.ToArray());
        }

        private async Task ProcessAsync(Server actor, byte[] packet)
        {
            // Validate connection
            if (!actor.Socket.Connected)
                return;

            var length = BitConverter.ToUInt16(packet, 0);
            var type = (PacketType) BitConverter.ToUInt16(packet, 2);

            try
            {
                MsgBase<Server> msg = null;
                switch (type)
                {
                    case PacketType.MsgUpdLoginEx:
                    {
                        msg = new MsgUpdLoginEx();
                        break;
                    }
                    default:
                        await Log.WriteLogAsync(LogLevel.Warning,
                                                "Missing packet {0}, Length {1}\n{2}",
                                                type, length, PacketDump.Hex(packet));
                        return;
                }

                // Decode packet bytes into the structure and process
                msg.Decode(packet);
                await msg.ProcessAsync(actor);
            }
            catch (Exception e)
            {
                await Log.WriteLogAsync(LogLevel.Exception, e.Message);
            }
        }

        protected override void Disconnected(Server actor)
        {
            _ = mThread.OnDisconnectAsync().ConfigureAwait(true);
        }
    }
}

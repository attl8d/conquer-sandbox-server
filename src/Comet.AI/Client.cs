using Comet.AI.Packets;
using Comet.AI.States;
using Comet.Network.Packets;
using Comet.Network.Sockets;
using Comet.Shared;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static Comet.AI.Database.ServerConfiguration;

namespace Comet.AI
{
    public sealed class Client : TcpClientWrapper<Server>
    {
        public static GameNetworkConfiguration Configuration;

        private readonly PacketProcessor<Server> Processor;

        public Client()
            : base("8a653a5d1e92b4e1db79".Length)
        {
            Processor = new PacketProcessor<Server>(ProcessAsync);
            Processor.StartAsync(CancellationToken.None).ConfigureAwait(false);
        }

        protected override async Task<Server> ConnectedAsync(Socket socket, Memory<byte> buffer)
        {
            Server client = new(socket, buffer, 0);
            if (socket.Connected)
            {
                Kernel.GameClient = this;
                return client;
            }
            return null;
        }

        protected override void Received(Server actor, ReadOnlySpan<byte> packet)
        {
            Kernel.NetworkMonitor.Receive(packet.Length);
            Processor.Queue(actor, packet.ToArray());
        }

        private async Task ProcessAsync(Server actor, byte[] packet)
        {
            // Validate connection
            if (!actor.Socket.Connected)
                return;

            var length = BitConverter.ToUInt16(packet, 0);
            PacketType type = (PacketType)BitConverter.ToUInt16(packet, 2);

            try
            {
                MsgBase<Server> msg = null;
                switch (type)
                {
                    case PacketType.MsgAiAction:
                        {
                            msg = new MsgAiAction();
                            break;
                        }

                    case PacketType.MsgAiLoginExchangeEx:
                        {
                            msg = new MsgAiLoginExchangeEx();
                            break;
                        }

                    case PacketType.MsgAiPlayerLogin:
                        {
                            msg = new MsgAiPlayerLogin();
                            break;
                        }

                    case PacketType.MsgAiPlayerLogout:
                        {
                            msg = new MsgAiPlayerLogout();
                            break;
                        }

                    case PacketType.MsgAiSpawnNpc:
                        {
                            msg = new MsgAiSpawnNpc();
                            break;
                        }

                    case PacketType.MsgAiRoleStatusFlag:
                        {
                            msg = new MsgAiRoleStatusFlag();
                            break;
                        }

                    case PacketType.MsgAiDynaMap:
                        {
                            msg = new MsgAiDynaMap();
                            break;
                        }

                    case PacketType.MsgAiGeneratorManage:
                        {
                            msg = new MsgAiGeneratorManage();
                            break;
                        }

                    case PacketType.MsgInteract:
                        {
                            msg = new MsgInteract();
                            break;
                        }

                    case PacketType.MsgAction:
                        {
                            msg = new MsgAction();
                            break;
                        }

                    case PacketType.MsgWalk:
                        {
                            msg = new MsgWalk();
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
            Kernel.GameClient = null;
            Kernel.GameServer = null;

            _ = Log.WriteLogAsync(LogLevel.Info, "Disconnected from the game server!").ConfigureAwait(false);
        }
    }
}

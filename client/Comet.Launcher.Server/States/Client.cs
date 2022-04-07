using System;
using System.Net.Sockets;
using Comet.Network.Sockets;

namespace Comet.Launcher.Server.States
{
    internal class Client : TcpServerActor
    {
        public Client(Socket socket, Memory<byte> buffer, uint partition)
            : base(socket, buffer, null, partition)
        {
            GUID = Guid.NewGuid().ToString();
        }

        public string GUID { get; }
    }
}
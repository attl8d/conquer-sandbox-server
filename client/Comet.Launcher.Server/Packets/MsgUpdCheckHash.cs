using System;
using System.IO;
using System.Threading.Tasks;
using Comet.Launcher.Files.Helpers;
using Comet.Launcher.Server.States;
using Comet.Network.Packets.Updater;

namespace Comet.Launcher.Server.Packets
{
    internal sealed class MsgUpdCheckHash : MsgUpdCheckHash<Client>
    {
        /// <inheritdoc />
        public override async Task ProcessAsync(Client client)
        {
            var success = true;
            foreach (FileHash file in Hashes)
            {
                string path = Path.Combine(Environment.CurrentDirectory, file.FilePath);
                if (!path.GetMd5().Equals(file.Hash))
                {
                    success = false;
                }
                else
                {
                    if (file.FilePath.ToUpperInvariant().EndsWith("CONQUER.EXE"))
                    {
                        client.ConquerHashOk = true;
                    }
                }
            }

            if (!Eof)
                return;

            int errorResult;
            do
            {
                errorResult = await Kernel.NextAsync(1, int.MaxValue);
            }
            while (errorResult == MsgUpdConfirmHash.SUCCESS);

            if (!success)
            {
                await client.SendAsync(new MsgUpdConfirmHash
                {
                    Result = errorResult
                });
                client.Disconnect();
                return;
            }

            await client.SendAsync(new MsgUpdConfirmHash
            {
                Result = client.ConquerHashOk ? MsgUpdConfirmHash.SUCCESS : errorResult
            });
        }
    }
}
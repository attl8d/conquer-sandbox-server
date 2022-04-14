using Comet.Launcher.Files.Helpers;
using Comet.Launcher.States;
using Comet.Network.Packets.Updater;

namespace Comet.Launcher.Packets
{
    internal sealed class MsgUpdPatchList : MsgUpdPatchList<Server>
    {
        private readonly string[] mSendFileList =
        {
            "Conquer.exe"
        };

        /// <inheritdoc />
        public override async Task ProcessAsync(Server client)
        {
            if (Mode == MsgUpdPatchType.NoUpdate)
            {
                var msg = new MsgUpdCheckHash
                {
                    Eof = true
                };
                foreach (string file in mSendFileList)
                {
                    if (!File.Exists(Path.Combine(Environment.CurrentDirectory, file)))
                    {
                        client.Disconnect();
                        return;
                    }

                    string hash = Path.Combine(Environment.CurrentDirectory, file).GetMd5();
                    msg.Hashes.Add(new MsgUpdCheckHash<Server>.FileHash
                    {
                        FilePath = file,
                        Hash = hash
                    });
                }

                if (msg.Hashes.Count < 1)
                {
                    client.Disconnect();
                    return;
                }

                await client.SendAsync(msg);
                return;
            }

            switch (Mode)
            {
                case MsgUpdPatchType.Client:
                {
                    break;
                }

                case MsgUpdPatchType.Game:
                {
                    break;
                }
            }
        }
    }
}
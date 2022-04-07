using System.Runtime.InteropServices;
using Comet.Launcher.Configuration;
using Comet.Launcher.Files.Helpers;
using Comet.Launcher.Helpers;
using Comet.Launcher.Managers;
using Comet.Launcher.Packets;
using Comet.Launcher.States;
using Comet.Network.Helpers;
using Comet.Shared;

namespace Comet.Launcher.Threads
{
    public sealed class LauncherThread : TimerBase
    {
        private FrmMain mForm;

        private Client mClient;
        private Server mServer;

        /// <inheritdoc />
        public LauncherThread(FrmMain form)
            : base(1000, "LauncherThread")
        {
            mForm = form;
        }

        /// <inheritdoc />
        protected override Task OnStartAsync()
        {
            if (OperatingSystemHelper.IsWindows7())
            {
                MessageBox.Show(LocaleManager.GetString("StrWindows7Alert"),
                    LocaleManager.GetString("StrWindows7AlertTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (OperatingSystemHelper.IsWindows7OrHigher() && Kernel.UserConfiguration.WindowsDefendAlert == null)
            {
                DialogResult result = MessageBox.Show(LocaleManager.GetString("StrWindows10Alert"),
                                LocaleManager.GetString("StrWindows10AlertTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    Kernel.OpenUrl("https://worldconqueronline.tawk.help/article/resolvendo-problemas-de-instalação");
                }

                Kernel.UserConfiguration.WindowsDefendAlert = DateTime.Now;
                Kernel.UserConfiguration.Save();
            }

            return base.OnStartAsync();
        }

        /// <inheritdoc />
        protected override async Task<bool> OnElapseAsync()
        {
            if (mClient == null)
            {
                mForm.SetProgressLabel(LocaleManager.GetString("StrAttemptingConnection"));
                foreach (string fullAddress in Kernel.ClientConfiguration.Addresses)
                {
                    string[] addresses = fullAddress.Split(':');
                    if (addresses.Length != 2)
                        continue;

                    string address = addresses[0];
                    if (!int.TryParse(addresses[1], out int port))
                        continue;

                    Client client = new (this);
                    if (!await client.ConnectToAsync(address, port))
                        continue;

                    mForm.SetProgressLabel("StrConnectedToServer");
                    mClient = client;
                    break;
                }
            }
            else if (mServer != null && !mServer.Socket.Connected)
            {
                mServer.Disconnect();
            }

            return true;
        }

        /// <inheritdoc />
        protected override Task OnCloseAsync()
        {
            return base.OnCloseAsync();
        }

        public async Task OnConnectAsync(Server server)
        {
            mServer = server;

            string conquerExeMd5 = "Conquer.exe".GetMd5();
            await mServer.SendAsync(new MsgUpdLogin
            {
                CurrentFileMd5 = conquerExeMd5,
                MacAddress = NetworkHelper.GetLocalMacAddress,
                IpAddresses = NetworkHelper.GetLocalIpAddresses,
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                WindowsVersion = RuntimeInformation.OSDescription,
                MachineDomain = Environment.UserDomainName
            });
        }

        public async Task OnDisconnectAsync()
        {
            mForm.SetProgressLabel("StrDisconnectedFromServer");

            mClient = null;
            mServer = null;
        }
    }
}
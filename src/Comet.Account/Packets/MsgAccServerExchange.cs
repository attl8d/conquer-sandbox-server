using System;
using Comet.Account.Database.Models;
using Comet.Account.States;
using Comet.Network.Packets.Internal;
using System.Linq;
using System.Threading.Tasks;
using Comet.Shared;
using Comet.Account.Database;
using Comet.Network.Security;

namespace Comet.Account.Packets
{
    public sealed class MsgAccServerExchange : MsgAccServerExchange<GameServer>
    {
        public static byte[] RealmDataKey { get; set; }

        public override async Task ProcessAsync(GameServer client)
        {
            DbRealm realm = Kernel.Realms.Values.FirstOrDefault(x => x.Name.Equals(ServerName, StringComparison.InvariantCultureIgnoreCase));

            if (realm == null)
            {
                await client.SendAsync(new MsgAccServerAction
                {
                    Action = MsgAccServerAction<GameServer>.ServerAction.ConnectionResult,
                    Data = (int)MsgAccServerAction<GameServer>.ConnectionStatus.AuthorizationError
                });
                client.Disconnect();
                return;
            }

            string username = AesCipher.Decrypt(RealmDataKey, realm.Username);
            string password = AesCipher.Decrypt(RealmDataKey, realm.Password);

            if (!Username.Equals(username, StringComparison.InvariantCulture) || !Password.Equals(password, StringComparison.InvariantCulture))
            {
                await client.SendAsync(new MsgAccServerAction
                {
                    Action = MsgAccServerAction<GameServer>.ServerAction.ConnectionResult,
                    Data = (int)MsgAccServerAction<GameServer>.ConnectionStatus.InvalidUsernamePassword
                });
                client.Disconnect();
                return;
            }

            if (!client.IPAddress.Equals(realm.RpcIPAddress))
            {
                await client.SendAsync(new MsgAccServerAction
                {
                    Action = MsgAccServerAction<GameServer>.ServerAction.ConnectionResult,
                    Data = (int)MsgAccServerAction<GameServer>.ConnectionStatus.AddressNotAuthorized
                });
                client.Disconnect();
                return;
            }

            await Log.WriteLogAsync(LogLevel.Info, $"Server [{realm.Name}] has authenticated gracefully.");
            realm.Server = client;
            client.SetRealm(realm.Name);

            realm.LastPing = DateTime.Now;
            realm.Status = DbRealm.RealmStatus.Online;
            await BaseRepository.SaveAsync(realm);

            await client.SendAsync(new MsgAccServerAction
            {
                Action = MsgAccServerAction<GameServer>.ServerAction.ConnectionResult,
                Data = (int)MsgAccServerAction<GameServer>.ConnectionStatus.Success
            });
        }
    }
}

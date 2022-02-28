using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Comet.Game.Packets;
using Comet.Game.States;
using Comet.Network.Packets;
using Comet.Shared;

namespace Comet.Game.World.Maps
{
    public sealed class Screen
    {
        public const int VIEW_SIZE = 18;
        public const int BROADCAST_SIZE = 21;

        private readonly Role mRole;

        public ConcurrentDictionary<uint, Role> Roles = new ConcurrentDictionary<uint, Role>();

        public Screen(Role role)
        {
            mRole = role;
        }

        public bool Add(Role role)
        {
            return Roles.TryAdd(role.Identity, role);
        }

        public async Task RemoveAsync(uint idRole, bool force = false)
        {
            Roles.TryRemove(idRole, out var role);

            if (force)
            {
                MsgAction msg = new MsgAction
                {
                    Identity = idRole,
                    Action = MsgAction.ActionType.RemoveEntity
                };
                await mRole.SendAsync(msg);
            }
        }

        public async Task SynchroScreenAsync()
        {
            if (mRole is not Character player)
                return;

            foreach (var role in Roles.Values)
            {                
                await role.SendSpawnToAsync(player);

                if (role is Character user)
                    await mRole.SendSpawnToAsync(user);
            }
        }

        public async Task UpdateAsync(IPacket msg = null)
        {
            var targets = mRole.Map.Query9BlocksByPos(mRole.MapX, mRole.MapY);
            targets.AddRange(Roles.Values);
            foreach (Role target in targets.Select(x => x).Distinct())
            {
                if (target.Identity == mRole.Identity) continue;

                Character targetUser = target as Character;
                if (Calculations.GetDistance(mRole.MapX, mRole.MapY, target.MapX, target.MapY) <= mRole.ViewRange)
                {
                    /*
                     * I add the target to my screen and it doesn't matter if he already sees me, I'll try to add myself into his screen.
                     * If succcess, I exchange the spawns.
                     */
                    if (Add(target))
                    {
                        target.Screen?.Add(mRole);

                        if (mRole is Character user)
                            await target.SendSpawnToAsync(user);
                        if (targetUser != null)
                            await mRole.SendSpawnToAsync(targetUser);
                    }
                }
                else
                {
                    await RemoveAsync(target.Identity);
                    if (target.Screen != null)
                    await target.Screen.RemoveAsync(mRole.Identity);
                }

                if (msg != null && targetUser != null)
                    await targetUser.SendAsync(msg);
            }
        }

        public async Task BroadcastRoomMsgAsync(IPacket msg, bool self = true)
        {
            if (self && mRole != null)
                await mRole.SendAsync(msg);

            foreach (var target in Roles.Values.Where(x => x is Character).Cast<Character>())
                await target.SendAsync(msg);
        }

        /// <summary>
        ///     For roles (not users) entering the screen.
        /// </summary>
        public async Task<bool> SpawnAsync(Role role)
        {
            if (mRole is not Character user)
                return false;

            if (Roles.TryAdd(role.Identity, role))
            {
                await role.SendSpawnToAsync(user);
                return true;
            }

            return false;
        }

        public async Task ClearAsync(bool sync = false)
        {
            if (sync && mRole is Character)
            {
                foreach (var role in Roles.Values)
                {
                    MsgAction msg = new MsgAction
                    {
                        Identity = role.Identity,
                        Action = MsgAction.ActionType.RemoveEntity
                    };
                    await mRole.SendAsync(msg);
                }
            }

            Roles.Clear();
        }
    }
}
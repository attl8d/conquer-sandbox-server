using Comet.Core;
using Comet.Game.States;
using Comet.Game.States.Items;
using Comet.Game.World.Managers;
using Comet.Game.World.Maps;
using Comet.Network.Packets;
using Comet.Network.Packets.Game;
using Comet.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using static Comet.Network.Packets.Game.MsgName<Comet.Game.States.Client>;

namespace Comet.Game.Packets
{
    public sealed class MsgInteract : MsgInteract<Client>
    {
        /// <summary>
        ///     Process can be invoked by a packet after decode has been called to structure
        ///     packet fields and properties. For the server implementations, this is called
        ///     in the packet handler after the message has been dequeued from the server's
        ///     <see cref="PacketProcessor{TClient}" />.
        /// </summary>
        /// <param name="client">Client requesting packet processing</param>
        public override async Task ProcessAsync(Client client)
        {
            Character user = client?.Character;

            Role sender = RoleManager.GetRole(SenderIdentity);
            if (sender == null || (user != null && user.Identity != SenderIdentity))
                return;

            if (user != null && SenderIdentity == user.Identity && !user.IsAlive)
            {
                await user.SendAsync(Language.StrDead);
                return;
            }

            if (sender.QueryStatus(StatusSet.FREEZE) != null
                || sender.QueryStatus(StatusSet.ICE_BLOCK) != null
                || sender.QueryStatus(StatusSet.DAZED) != null
                || sender.QueryStatus(StatusSet.HUGE_DAZED) != null
                || sender.QueryStatus(StatusSet.CONFUSED) != null)
                return;

            switch (Action)
            {
                case MsgInteractType.Attack:
                case MsgInteractType.Shoot:
                case MsgInteractType.MagicAttack:
                case MsgInteractType.AbortMagic:
                case MsgInteractType.Court:
                case MsgInteractType.Marry:
                    {
                        sender.BattleSystem.ResetBattle();
                        if (sender.MagicData.QueryMagic != null)
                            await sender.MagicData.AbortMagicAsync(true);
                        break;
                    }
            }

            Role target = sender?.Map.QueryAroundRole(sender, TargetIdentity);

            if (target == null && Action != MsgInteractType.MagicAttack)
                return;

            switch (Action)
            {
                case MsgInteractType.Attack:
                case MsgInteractType.Shoot:
                    {
                        if (!sender.IsAlive)
                            return;

                        if (user != null)
                        {
                            if (!await user.SynPositionAsync(PosX, PosY, 8))
                            {
                                await RoleManager.KickOutAsync(sender.Identity, "MsgInteract SynPosition");
                                return;
                            }
                        }

                        if (sender.SetAttackTarget(target))
                            sender.BattleSystem.CreateBattle(TargetIdentity);
                        break;
                    }

                case MsgInteractType.MagicAttack:
                    {
                        if (!sender.IsAlive)
                            return;

                        if (client != null)
                        {
                            byte[] dataArray = BitConverter.GetBytes(Data);
                            Data = Convert.ToUInt16((dataArray[0] & 0xFF) | ((dataArray[1] & 0xFF) << 8));
                            Data ^= 0x915d;
                            Data ^= (ushort)client.Identity;
                            Data = (ushort)((Data << 0x3) | (Data >> 0xd));
                            Data -= 0xeb42;

                            dataArray = BitConverter.GetBytes(TargetIdentity);
                            TargetIdentity = ((uint)dataArray[0] & 0xFF) | (((uint)dataArray[1] & 0xFF) << 8) |
                                             (((uint)dataArray[2] & 0xFF) << 16) | (((uint)dataArray[3] & 0xFF) << 24);
                            TargetIdentity = ((((TargetIdentity & 0xffffe000) >> 13) | ((TargetIdentity & 0x1fff) << 19)) ^
                                              0x5F2D2463 ^ client.Identity) -
                                             0x746F4AE6;

                            dataArray = BitConverter.GetBytes(PosX);
                            long xx = (dataArray[0] & 0xFF) | ((dataArray[1] & 0xFF) << 8);
                            dataArray = BitConverter.GetBytes(PosY);
                            long yy = (dataArray[0] & 0xFF) | ((dataArray[1] & 0xFF) << 8);
                            xx = xx ^ (client.Identity & 0xffff) ^ 0x2ed6;
                            xx = ((xx << 1) | ((xx & 0x8000) >> 15)) & 0xffff;
                            xx |= 0xffff0000;
                            xx -= 0xffff22ee;
                            yy = yy ^ (client.Identity & 0xffff) ^ 0xb99b;
                            yy = ((yy << 5) | ((yy & 0xF800) >> 11)) & 0xffff;
                            yy |= 0xffff0000;
                            yy -= 0xffff8922;
                            PosX = Convert.ToUInt16(xx);
                            PosY = Convert.ToUInt16(yy);
                        }

                        await sender.ProcessMagicAttackAsync((ushort)Data, TargetIdentity, PosX, PosY);
                        break;
                    }

                case MsgInteractType.Chop:
                    {
                        if (user == null)
                            return;

                        Item targetItem = user.UserPackage.GetItemByType(Item.TYPE_JAR);
                        if (targetItem == null)
                            return;

                        Command = (ushort)(targetItem.Data * 2);
                        await client.SendAsync(this);

                        break;
                    }

                case MsgInteractType.Court:
                    {
                        if (user == null)
                            return;

                        if (target == null || target.Identity == user.Identity)
                            return;

                        if (!(target is Character targetUser))
                            return;

                        if (targetUser.MapIdentity != user.MapIdentity || user.GetDistance(targetUser) > Screen.VIEW_SIZE)
                        {
                            await user.SendAsync(Language.StrTargetNotInRange);
                            return;
                        }

                        if (targetUser.MateIdentity != 0)
                        {
                            await user.SendAsync(Language.StrMarriageTargetNotSingle);
                            return; // target is already married
                        }

                        if (user.MateIdentity != 0)
                        {
                            await user.SendAsync(Language.StrMarriageYouNoSingle);
                            return; // you're already married
                        }

                        if (user.Gender == targetUser.Gender)
                        {
                            await user.SendAsync(Language.StrMarriageErrSameGender);
                            return; // not allow same gender
                        }

                        targetUser.SetRequest(RequestType.Marriage, user.Identity);
                        await targetUser.SendAsync(this);
                        break;
                    }

                case MsgInteractType.Marry:
                    {
                        if (user == null)
                            return;

                        if (target == null || target.Identity == user.Identity)
                            return;

                        if (!(target is Character targetUser))
                            return;

                        if (user.QueryRequest(RequestType.Marriage) != targetUser.Identity)
                        {
                            await user.SendAsync(Language.StrMarriageNotApply);
                            return;
                        }

                        user.PopRequest(RequestType.Marriage);

                        if (targetUser.MapIdentity != user.MapIdentity || user.GetDistance(targetUser) > Screen.VIEW_SIZE)
                        {
                            await user.SendAsync(Language.StrTargetNotInRange);
                            return;
                        }

                        if (targetUser.MateIdentity != 0)
                        {
                            await user.SendAsync(Language.StrMarriageTargetNotSingle);
                            return; // target is already married
                        }

                        if (user.MateIdentity != 0)
                        {
                            await user.SendAsync(Language.StrMarriageYouNoSingle);
                            return; // you're already married
                        }

                        if (user.Gender == targetUser.Gender)
                        {
                            await user.SendAsync(Language.StrMarriageErrSameGender);
                            return; // not allow same gender
                        }

                        user.MateIdentity = targetUser.Identity;
                        user.MateName = targetUser.Name;
                        await user.SaveAsync();
                        targetUser.MateIdentity = user.Identity;
                        targetUser.MateName = user.Name;
                        await targetUser.SaveAsync();

                        await user.SendAsync(new MsgName
                        {
                            Identity = user.Identity,
                            Strings = new List<string> { targetUser.Name },
                            Action = StringAction.Mate
                        });

                        await targetUser.SendAsync(new MsgName
                        {
                            Identity = targetUser.Identity,
                            Strings = new List<string> { user.Name },
                            Action = StringAction.Mate
                        });

                        await user.BroadcastRoomMsgAsync(new MsgItem
                        {
                            Action = MsgItem<Client>.ItemActionType.Fireworks,
                            Identity = user.Identity
                        }, false);

                        await targetUser.BroadcastRoomMsgAsync(new MsgItem
                        {
                            Action = MsgItem<Client>.ItemActionType.Fireworks,
                            Identity = targetUser.Identity
                        }, false);
                        await RoleManager.BroadcastMsgAsync(string.Format(Language.StrMarry, targetUser.Name, user.Name), TalkChannel.Center, Color.Red);
                        break;
                    }

                case MsgInteractType.InitialMerchant:
                case MsgInteractType.AcceptMerchant:
                    {
                        if (user == null)
                            return;

                        // ON ACCEPT: Sender = 1 Target = 1
                        await user.SetMerchantAsync();
                        break;
                    }

                case MsgInteractType.CancelMerchant:
                    {
                        if (user == null)
                            return;

                        await user.RemoveMerchantAsync();
                        break;
                    }

                case MsgInteractType.MerchantProgress:
                    {
                        break;
                    }

                default:
                    {
                        if (client != null)
                        {
                            await client.SendAsync(new MsgTalk(client.Identity, TalkChannel.Service,
                                $"Missing packet {Type}, Action {Action}, Length {Length}"));
                        }
                        await Log.WriteLogAsync(LogLevel.Warning,
                            "Missing packet {0}, Action {1}, Length {2}\n{3}",
                            Type, Action, Length, PacketDump.Hex(Encode()));
                        break;
                    }
            }
        }
    }
}

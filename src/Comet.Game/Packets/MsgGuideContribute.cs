using Comet.Core;
using Comet.Game.States;
using Comet.Game.States.Items;
using Comet.Network.Packets;
using Comet.Network.Packets.Game;
using Comet.Shared;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgGuideContribute : MsgGuideContribute<Client>
    {
        public override async Task ProcessAsync(Client client)
        {
            Character user = client.Character;

            switch (Mode)
            {
                case RequestType.Query:
                    {
                        Experience = (uint)user.MentorExpTime;
                        Composing = (ushort)user.MentorAddLevexp;
                        HeavenBlessing = (ushort)user.MentorGodTime;
                        await client.SendAsync(this);
                        break;
                    }

                case RequestType.ClaimExperience:
                    {
                        if (user.MentorExpTime > 0)
                        {
                            await user.AwardExperienceAsync(user.CalculateExpBall((int)user.MentorExpTime), true);

                            user.MentorExpTime = 0;
                            await user.SaveTutorAccessAsync();
                        }
                        break;
                    }

                case RequestType.ClaimHeavenBlessing:
                    {
                        if (user.MentorGodTime > 0)
                        {
                            await user.AddBlessingAsync(user.MentorGodTime);
                            user.MentorGodTime = 0;
                            await user.SaveTutorAccessAsync();
                        }

                        break;
                    }

                case RequestType.ClaimItemAdd:
                    {
                        int stoneAmount = (int)(user.MentorAddLevexp / 100);

                        if (!user.UserPackage.IsPackSpare(stoneAmount))
                        {
                            await user.SendAsync(Language.StrYourBagIsFull);
                            return;
                        }

                        for (int i = 0; i < stoneAmount; i++)
                        {
                            if (await user.UserPackage.AwardItemAsync(Item.TYPE_STONE1))
                                user.MentorAddLevexp -= 100;
                        }

                        await user.SaveTutorAccessAsync();
                        break;
                    }

                default:
                    {
                        await client.SendAsync(this);
                        if (client.Character.IsPm())
                        {
                            await client.SendAsync(new MsgTalk(client.Identity, TalkChannel.Service,
                                $"Missing packet {Type}, Action {Mode}, Length {Length}"));
                        }

                        await Log.WriteLogAsync(LogLevel.Warning,
                            "Missing packet {0}, Action {1}, Length {2}\n{3}",
                            Type, Mode, Length, PacketDump.Hex(Encode()));
                        break;
                    }
            }
        }
    }
}

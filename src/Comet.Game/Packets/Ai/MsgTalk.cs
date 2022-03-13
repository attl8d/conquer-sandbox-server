using System.Drawing;
using System.Threading.Tasks;
using Comet.Game.Internal.AI;
using Comet.Game.States;
using Comet.Game.World.Managers;
using Comet.Network.Packets.Game;

namespace Comet.Game.Packets.Ai
{
    /// <remarks>Packet Type 1004</remarks>
    /// <summary>
    ///     Message defining a chat message from one player to the other, or from the system
    ///     to a player. Used for all chat systems in the game, including messages outside of
    ///     the game world state, such as during character creation or to tell the client to
    ///     continue logging in after connect.
    /// </summary>
    public sealed class MsgTalk : MsgTalk<AiClient>
    {
        public MsgTalk()
        {
        }

        public MsgTalk(uint characterID, TalkChannel channel, string text)
            : base(characterID, channel, text)
        {
        }

        public MsgTalk(uint characterID, TalkChannel channel, Color color, string text)
            : base(characterID, channel, color, text)
        {
        }

        public MsgTalk(uint characterID, TalkChannel channel, Color color, string recipient, string sender, string text)
            : base(characterID, channel, color, recipient, sender, text)
        {
        }

        public static MsgTalk LoginOk { get; } = new(0, TalkChannel.Login, "ANSWER_OK");
        public static MsgTalk LoginInvalid { get; } = new(0, TalkChannel.Login, "Invalid login");
        public static MsgTalk LoginNewRole { get; } = new(0, TalkChannel.Login, "NEW_ROLE");
        public static MsgTalk RegisterOk { get; } = new(0, TalkChannel.Register, "ANSWER_OK");
        public static MsgTalk RegisterInvalid { get; } = new(0, TalkChannel.Register, "Invalid character");
        public static MsgTalk RegisterNameTaken { get; } = new(0, TalkChannel.Register, "Character name taken");
        public static MsgTalk RegisterTryAgain { get; } = new(0, TalkChannel.Register, "Error, please try later");

        public override async Task ProcessAsync(AiClient client)
        {
            Role sender = RoleManager.GetRole(CharacterID);

            switch (Channel)
            {
                case TalkChannel.Talk:
                {
                    if (!sender.IsAlive)
                        return;

                    await sender.BroadcastRoomMsgAsync(this, false);
                    break;
                }
            }
        }
    }
}
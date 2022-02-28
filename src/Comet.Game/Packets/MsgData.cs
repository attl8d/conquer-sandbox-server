using Comet.Game.States;
using Comet.Network.Packets.Game;
using System;

namespace Comet.Game.Packets
{
    public sealed class MsgData : MsgData<Client>
    {
        public MsgData()
            : base(DateTime.Now)
        {

        }
    }
}

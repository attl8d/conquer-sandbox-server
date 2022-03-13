using System.Threading.Tasks;
using Comet.AI.Packets;
using Comet.AI.World.Managers;

namespace Comet.AI.States
{
    public sealed class Character : Role
    {
        private int mBattlePower;

        public int Metempsychosis { get; set; }
        public override int BattlePower => mBattlePower;
        public override uint MaxLife { get; }

        public override bool IsAlive => QueryStatus(StatusSet.GHOST) == null;
        public int Silvers { get; set; }
        public int ConquerPoints { get; set; }
        public int Nobility { get; set; }
        public int Syndicate { get; set; }
        public int SyndicatePosition { get; set; }
        public int Family { get; set; }
        public int FamilyPosition { get; set; }

        public async Task<bool> InitializeAsync(MsgAiPlayerLogin msg)
        {
            Identity = msg.Id;
            Name = msg.Name;
            Level = (byte) msg.Level;
            Metempsychosis = msg.Metempsychosis;
            StatusFlag = msg.Flag1;
            mBattlePower = msg.BattlePower;
            Life = (uint) msg.Life;
            Silvers = msg.Money;
            ConquerPoints = msg.ConquerPoints;
            Nobility = msg.Nobility;
            Syndicate = msg.Syndicate;
            SyndicatePosition = msg.SyndicatePosition;
            Family = msg.Family;
            FamilyPosition = msg.FamilyPosition;
            MapIdentity = msg.MapId;
            MapX = msg.X;
            MapY = msg.Y;

            if ((Map = MapManager.GetMap(msg.MapId)) == null)
                return false;

            await EnterMapAsync(false);
            return true;
        }

        public async Task<bool> LogoutAsync()
        {
            await LeaveMapAsync(false);
            return true;
        }
    }
}
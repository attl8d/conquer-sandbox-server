using Comet.Database.Entities;
using Comet.Shared;
using System;

namespace Comet.AI.States
{
    public sealed class MonsterMagic
    {
        private TimeOutMS timeOutMS = new TimeOutMS();

        public MonsterMagic(DbMonsterTypeMagic magic)
        {
            MonsterType = magic.MonsterType;
            MagicType = magic.MagicType;
            MagicLev = magic.MagicLev;
            ColdTime = magic.ColdTime;
            WarningTime = magic.WarningTime;
            StatusMagicLev = magic.StatusMagicLev;
            StatusMagicType = magic.StatusMagicType;
            LastTick = 0;

            timeOutMS.Startup((int)ColdTime);
        }

        public uint MonsterType { get; private set; }
        public uint MagicType { get; private set; }
        public uint MagicLev { get; private set; }
        public uint ColdTime { get; private set; }
        public ushort WarningTime { get; private set; }
        public uint StatusMagicType { get; private set; }
        public uint StatusMagicLev { get; private set; }
        public long LastTick { get; set; }

        public bool IsReady() => timeOutMS.IsTimeOut();
        public void Use()
        {
            timeOutMS.Update();
            LastTick = Environment.TickCount64;
        }
    }
}

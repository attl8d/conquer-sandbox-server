using Comet.Database.Entities;

namespace Comet.Account.States
{
    public sealed class Player
    {
        public DbRealm Realm;
        public uint AccountIdentity;
        public DbAccount Account;
    }
}
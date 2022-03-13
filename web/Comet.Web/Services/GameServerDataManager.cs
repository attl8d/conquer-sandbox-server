using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Services.Realms;
using static Comet.Web.Data.Entities.LogHistory;

namespace Comet.Web.Services
{
    public sealed class GameServerDataManager
    {
        private readonly ApplicationDbContext mContext;
        private readonly LogService<GameServerDataManager> mLog;
        private readonly RealmService mRealmService;

        public GameServerDataManager(RealmService realmService, ApplicationDbContext ctx,
                                     LogService<GameServerDataManager> log)
        {
            mRealmService = realmService;
            mContext = ctx;
            mLog = log;
        }

        public async Task ExecuteMigrationAsync()
        {
            await mLog.LogAsync("Daily Update initializing!", LogType.DailyUpdate);
            var sw = Stopwatch.StartNew();

            await mRealmService.LoadRealmsAsync();

            foreach (RealmService.RealmPublicListData realm in mRealmService.GetRealmList().Where(x => x.Active))
            {
                await mLog.LogAsync($"Loading data from realm {realm.RealmName}", LogType.DailyUpdate);

                var dbContext = new ApplicationDbContext();
                dbContext.ConnectionString = mRealmService.GetRealmConnectionString(realm.RealmName);

                DataTable data = await dbContext.SelectAsync("CALL GetSyndicates();");
                var saveOrUpdateSyndicateList = new List<RecordSyndicate>();
                foreach (DataRow row in data.Rows)
                {
                    uint idSyndicate = uint.Parse(row["Identity"].ToString());
                    RecordSyndicate syndicate =
                        mContext.RecordSyndicates.FirstOrDefault(
                            x => x.ServerIdentity == realm.RealmID && x.SyndicateIdentity == idSyndicate);
                    if (syndicate == null)
                        syndicate = new RecordSyndicate
                        {
                            ServerIdentity = realm.RealmID,
                            SyndicateIdentity = idSyndicate
                        };
                    syndicate.Name = row.Field<string>("Name");
                    syndicate.LeaderIdentity = uint.Parse(row["LeaderIdentity"].ToString());
                    syndicate.CreatedAt = row.Field<DateTime>("CreatedAt");
                    syndicate.DeletedAt = row.Field<DateTime?>("DeletedAt");
                    syndicate.Money = long.Parse(row["Money"].ToString());
                    syndicate.Count = uint.Parse(row["Count"].ToString());
                    saveOrUpdateSyndicateList.Add(syndicate);
                }

                await mContext.SaveAsync(saveOrUpdateSyndicateList);
                saveOrUpdateSyndicateList.Clear();

                data = await dbContext.SelectAsync("CALL GetFamily();");
                var saveOrUpdateFamily = new List<RecordFamily>();
                foreach (DataRow row in data.Rows)
                {
                    uint idFamily = uint.Parse(row["Identity"].ToString());
                    RecordFamily family =
                        mContext.RecordFamilies.FirstOrDefault(
                            x => x.ServerIdentity == realm.RealmID && x.FamilyIdentity == idFamily);
                    if (family == null)
                        family = new RecordFamily
                        {
                            ServerIdentity = realm.RealmID,
                            FamilyIdentity = idFamily
                        };
                    family.Name = row.Field<string>("Name");
                    family.LeaderIdentity = uint.Parse(row["LeaderIdentity"].ToString());
                    family.CreatedAt = row.Field<DateTime>("CreatedAt");
                    family.DeletedAt = row.Field<DateTime?>("DeletedAt");
                    family.Money = ulong.Parse(row["Money"].ToString());
                    family.Count = byte.Parse(row["Count"].ToString());
                    family.ChallengeMap = uint.Parse(row["ChallengeMap"].ToString());
                    family.DominatedMap = uint.Parse(row["DominatedMap"].ToString());
                    family.BpTower = byte.Parse(row["BpTower"].ToString());
                    family.Level = byte.Parse(row["Level"].ToString());
                    saveOrUpdateFamily.Add(family);
                }

                await mContext.SaveAsync(saveOrUpdateFamily);
                saveOrUpdateFamily.Clear();

                data = await dbContext.SelectAsync("CALL GetUsers();");
                var saveOrUpdateUser = new List<RecordUser>();
                foreach (DataRow row in data.Rows)
                {
                    uint idUser = uint.Parse(row["Identity"].ToString());
                    RecordUser user =
                        mContext.RecordUsers.FirstOrDefault(x => x.ServerIdentity == realm.RealmID && x.Id == idUser);

                    if (user == null)
                        user = new RecordUser
                        {
                            UserIdentity = idUser,
                            ServerIdentity = realm.RealmID
                        };

                    user.Name = row.Field<string>("Name");
                    user.AccountIdentity = uint.Parse(row["AccountIdentity"].ToString());
                    user.MateId = uint.Parse(row["MateId"].ToString());
                    user.Level = byte.Parse(row["Level"].ToString());
                    user.Experience = ulong.Parse(row["Experience"].ToString());
                    user.Profession = byte.Parse(row["Profession"].ToString());
                    user.NewProfession = byte.Parse(row["OldProfession"].ToString());
                    user.OldProfession = byte.Parse(row["FirstProfession"].ToString());
                    user.Metempsychosis = byte.Parse(row["Metempsychosis"].ToString());
                    user.Strength = ushort.Parse(row["Strength"].ToString());
                    user.Agility = ushort.Parse(row["Agility"].ToString());
                    user.Vitality = ushort.Parse(row["Vitality"].ToString());
                    user.Spirit = ushort.Parse(row["Spirit"].ToString());
                    user.AdditionalPoints = ushort.Parse(row["AdditionalPoint"].ToString());
                    user.SyndicateIdentity = (uint) long.Parse(row["SyndicateIdentity"].ToString());
                    user.SyndicatePosition = (ushort) long.Parse(row["SyndicateRank"].ToString());
                    user.NobilityDonation = ulong.Parse(row["NobilityDonation"].ToString());
                    user.NobilityRank = (byte) long.Parse(row["NobilityRank"].ToString());
                    user.SupermanCount = (uint) long.Parse(row["SupermanCount"].ToString());
                    user.DeletedAt = row.Field<DateTime?>("DeletedAt");
                    user.Deleted = long.Parse(row["Deleted"].ToString()) != 0;
                    user.Money = uint.Parse(row["Money"].ToString());
                    user.WarehouseMoney = uint.Parse(row["WarehouseMoney"].ToString());
                    user.ConquerPoints = uint.Parse(row["ConquerPoints"].ToString());
                    user.ConquerPointsMono = uint.Parse(row["ConquerPointsMono"].ToString());
                    user.FamilyIdentity = (uint) long.Parse(row["FamilyIdentity"].ToString());
                    user.FamilyRank = (ushort) long.Parse(row["FamilyRank"].ToString());
                    user.QualifierPoints = uint.Parse(row["QualifierPoints"].ToString());
                    user.QualifierTotalWins = uint.Parse(row["QualifierTotalWins"].ToString());
                    user.QualifierTotalLoses = uint.Parse(row["QualifierTotalLoses"].ToString());
                    user.QualifierTodayWins = uint.Parse(row["QualifierTodayWins"].ToString());
                    user.QualifierTodayLoses = uint.Parse(row["QualifierTodayLoses"].ToString());
                    user.CurrentHonorPoint = uint.Parse(row["CurrentHonorPoint"].ToString());
                    user.HistoryHonorPoint = uint.Parse(row["HistoryHonorPoint"].ToString());
                    user.RedRose = uint.Parse(row["RedRose"].ToString());
                    user.WhiteRose = uint.Parse(row["WhiteRose"].ToString());
                    user.Orchid = uint.Parse(row["Orchid"].ToString());
                    user.Tulip = uint.Parse(row["Tulip"].ToString());
                }

                await mContext.SaveAsync(saveOrUpdateUser);
                saveOrUpdateUser.Clear();

                data = await dbContext.SelectAsync("CALL GetGuildWarWinners();");
            }

            sw.Stop();
            await mLog.LogAsync($"Finishing daily update in {sw.ElapsedMilliseconds / 1000d:0.000} seconds.",
                                LogType.DailyUpdate);
        }
    }
}
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services.Realms;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Comet.Web.App.Data.Entities.LogHistory;

namespace Comet.Web.App.Services.Schedule
{
    [DisallowConcurrentExecution]
    public class DailyGameSynchronizationJob : IJob
    {
        private readonly LogService<DailyGameSynchronizationJob> mLog;
        private readonly RealmService mRealmService;
        private readonly ApplicationDbContext mContext;

        public DailyGameSynchronizationJob(LogService<DailyGameSynchronizationJob> log,
            RealmService realmService,
            ApplicationDbContext ctx)
        {
            mLog = log;
            mRealmService = realmService;
            mContext = ctx;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await mLog.LogAsync($"Daily Update initializing!", LogType.DailyUpdate);
            Stopwatch sw = Stopwatch.StartNew();

            await mRealmService.LoadRealmsAsync();

            foreach (var realm in mRealmService.GetRealmList().Where(x => x.Active))
            {
                await mLog.LogAsync($"Loading data from realm {realm.RealmName}", LogType.DailyUpdate);

                ApplicationDbContext dbContext = new ApplicationDbContext();
                dbContext.ConnectionString = mRealmService.GetRealmConnectionString(realm.RealmName);

                DataTable data = await dbContext.SelectAsync("CALL GetSyndicates();");
                List<RecordSyndicate> saveOrUpdateSyndicateList = new List<RecordSyndicate>();
                foreach (DataRow row in data.Rows)
                {
                    uint idSyndicate = (uint)row.Field<int>("Identity");
                    var syndicate = mContext.RecordSyndicates.FirstOrDefault(x => x.ServerIdentity == realm.RealmID && x.SyndicateIdentity == idSyndicate);
                    if (syndicate == null)
                    {
                        syndicate = new RecordSyndicate
                        {
                            ServerIdentity = realm.RealmID,
                            SyndicateIdentity = idSyndicate
                        };
                    }
                    syndicate.Name = row.Field<string>("Name");
                    syndicate.LeaderIdentity = row.Field<uint>("LeaderIdentity");
                    syndicate.CreatedAt = row.Field<DateTime>("CreatedAt");
                    syndicate.DeletedAt = row.Field<DateTime?>("DeletedAt");
                    syndicate.Money = row.Field<long>("Money");
                    syndicate.Count = row.Field<uint>("Count");
                    saveOrUpdateSyndicateList.Add(syndicate);
                }
                await mContext.SaveAsync(saveOrUpdateSyndicateList);
                saveOrUpdateSyndicateList.Clear();

                data = await dbContext.SelectAsync("CALL GetFamily();");
                List<RecordFamily> saveOrUpdateFamily = new List<RecordFamily>();
                foreach (DataRow row in data.Rows)
                {
                    uint idFamily = (uint)row.Field<int>("Identity");
                    var family = mContext.RecordFamilies.FirstOrDefault(x => x.ServerIdentity == realm.RealmID && x.FamilyIdentity == idFamily);
                    if (family == null)
                    {
                        family = new RecordFamily
                        {
                            ServerIdentity = realm.RealmID,
                            FamilyIdentity = idFamily
                        };
                    }
                    family.Name = row.Field<string>("Name");
                    family.LeaderIdentity = row.Field<uint>("LeaderIdentity");
                    family.CreatedAt = row.Field<DateTime>("CreatedAt");
                    family.DeletedAt = row.Field<DateTime?>("DeletedAt");
                    family.Money = row.Field<ulong>("Money");
                    family.Count = row.Field<byte>("Count");
                    family.ChallengeMap = row.Field<uint>("ChallengeMap");
                    family.DominatedMap = row.Field<uint>("DominatedMap");
                    family.BpTower = row.Field<byte>("BpTower");
                    family.Level = row.Field<byte>("Level");
                    saveOrUpdateFamily.Add(family);
                }
                await mContext.SaveAsync(saveOrUpdateFamily);
                saveOrUpdateFamily.Clear();

                data = await dbContext.SelectAsync("CALL GetUsers();");
                List<RecordUser> saveOrUpdateUser = new List<RecordUser>();
                foreach (DataRow row in data.Rows)
                {
                    uint idUser = row.Field<uint>("Identity");
                    var user = mContext.RecordUsers.FirstOrDefault(x => x.ServerIdentity == realm.RealmID && x.Id == idUser);

                    if (user == null)
                    {
                        user = new RecordUser
                        {
                            UserIdentity = idUser,
                            ServerIdentity = realm.RealmID
                        };
                    }

                    user.Name = row.Field<string>("Name");
                    user.AccountIdentity = row.Field<uint>("AccountIdentity");
                    user.MateId = row.Field<uint>("MateId");
                    user.Level = row.Field<byte>("Level");
                    user.Experience = row.Field<ulong>("Experience");
                    user.Profession = row.Field<byte>("Profession");
                    user.NewProfession = row.Field<byte>("OldProfession");
                    user.OldProfession = row.Field<byte>("FirstProfession");
                    user.Metempsychosis = row.Field<byte>("Metempsychosis");
                    user.Strength = row.Field<ushort>("Strength");
                    user.Agility = row.Field<ushort>("Agility");
                    user.Vitality = row.Field<ushort>("Vitality");
                    user.Spirit = row.Field<ushort>("Spirit");
                    user.AdditionalPoints = row.Field<ushort>("AdditionalPoint");
                    user.SyndicateIdentity = (uint)row.Field<long>("SyndicateIdentity");
                    user.SyndicatePosition = (ushort)row.Field<long>("SyndicateRank");
                    user.NobilityDonation = row.Field<ulong>("NobilityDonation");
                    user.NobilityRank = (byte)row.Field<long>("NobilityRank");
                    user.SupermanCount = (uint)row.Field<long>("SupermanCount");
                    user.DeletedAt = row.Field<DateTime?>("DeletedAt");
                    user.Deleted = row.Field<long>("Deleted") != 0;
                    user.Money = row.Field<uint>("Money");
                    user.WarehouseMoney = row.Field<uint>("WarehouseMoney");
                    user.ConquerPoints = row.Field<uint>("ConquerPoints");
                    user.ConquerPointsMono = row.Field<uint>("ConquerPointsMono");
                    user.FamilyIdentity = (uint)row.Field<long>("FamilyIdentity");
                    user.FamilyRank = (ushort)row.Field<long>("FamilyRank");
                    user.QualifierPoints = row.Field<uint>("QualifierPoints");
                    user.QualifierTotalWins = row.Field<uint>("QualifierTotalWins");
                    user.QualifierTotalLoses = row.Field<uint>("QualifierTotalLoses");
                    user.QualifierTodayWins = row.Field<uint>("QualifierTodayWins");
                    user.QualifierTodayLoses = row.Field<uint>("QualifierTodayLoses");
                    user.CurrentHonorPoint = row.Field<uint>("CurrentHonorPoint");
                    user.HistoryHonorPoint = row.Field<uint>("HistoryHonorPoint");
                    user.RedRose = row.Field<uint>("RedRose");
                    user.WhiteRose = row.Field<uint>("WhiteRose");
                    user.Orchid = row.Field<uint>("Orchid");
                    user.Tulip = row.Field<uint>("Tulip");
                }
                await mContext.SaveAsync(saveOrUpdateUser);
                saveOrUpdateUser.Clear();

                data = await dbContext.SelectAsync("CALL GetGuildWarWinners();");
            }

            sw.Stop();
            await mLog.LogAsync($"Finishing daily update in {sw.ElapsedMilliseconds/1000d:0.000} seconds.", LogType.DailyUpdate);
        }
    }
}

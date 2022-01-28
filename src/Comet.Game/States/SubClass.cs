using Comet.Game.Database.Models;
using Comet.Game.Packets;
using Comet.Game.World.Managers;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using static Comet.Game.Packets.MsgSubPro;

namespace Comet.Game.States
{
    public sealed class SubClass
    {
        private ConcurrentDictionary<AstProfType, AstProf> mDictAstProfs = new ConcurrentDictionary<AstProfType, AstProf>();

        private readonly Character mOwner;

        public SubClass(Character owner)
        {
            mOwner = owner;
        }

        public async Task<bool> InitializeAsync()
        {
            var astProfs = await DbAstProfLevel.GetAsync(mOwner.Identity);
            foreach (var astProf in astProfs)
            {
                var type = (AstProfType)astProf.AstProf;
                if (!mDictAstProfs.ContainsKey(type))
                {
                    mDictAstProfs.TryAdd(type, new AstProf(astProf));
                }
            }

            foreach (var prof in mDictAstProfs.Values)
            {
                await mOwner.SendAsync(new MsgSubPro
                {
                    Action = AstProfAction.Learn,
                    Class = prof.Class,
                    Level = prof.Level
                });

                await mOwner.SendAsync(new MsgSubPro
                {
                    Action = AstProfAction.MartialPromoted,
                    Class = prof.Class,
                    Level = AstProfManager.GetRank(mOwner, prof.Class)
                });
            }
            return true;
        }

        public async Task<bool> LearnAsync(AstProfType type)
        {
            if (mDictAstProfs.ContainsKey(type))
                return false;

            if (!await AstProfManager.CanInaugurateAsync(mOwner, type))
                return false;

            var profession = new AstProf(type, mOwner.Identity);
            if (!mDictAstProfs.TryAdd(type, profession))
                return false;

            await profession.SaveAsync();

            AstProfManager.SetRank(mOwner, type, 1);
            await mOwner.SaveAsync();

            await mOwner.SendAsync(new MsgSubPro
            {
                Action = AstProfAction.Learn,
                Class = type
            });

            await SendAsync();

            if (mDictAstProfs.Count == 1)
                await ActivateAsync(type);

            return true;
        }

        public async Task<bool> UpLevAsync(AstProfType type)
        {
            if (!mDictAstProfs.ContainsKey(type))
                return false;

            var astProf = mDictAstProfs[type];
            if (!await AstProfManager.CanUpLevAsync(mOwner, type, astProf.Level))
                return false;

            astProf.Level += 1;
            await astProf.SaveAsync();
            await mOwner.SendAsync(new MsgSubPro
            {
                Action = AstProfAction.MartialUplev,
                Class = type
            });
            return true;
        }

        public async Task<bool> PromoteAsync(AstProfType type)
        {
            if (!mDictAstProfs.TryGetValue(type, out var profession) || AstProfManager.GetRank(mOwner, type) >= profession.Level)
                return false;

            if (!AstProfManager.CanPromote(mOwner, type, profession.Level))
                return false;

            int nextRank = AstProfManager.GetRank(mOwner, type) + 1;
            if (nextRank > 9)
                return false;

            AstProfManager.SetRank(mOwner, type, (byte)nextRank);
            await mOwner.SaveAsync();

            await mOwner.SendAsync(new MsgSubPro
            {
                Action = AstProfAction.MartialPromoted,
                Class = type,
                Level = (byte)nextRank
            });

            await SendAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(AstProfType type)
        {
            if (type != AstProfType.None && !mDictAstProfs.ContainsKey(type))
                return false;

            mOwner.AstProfType = type;
            await mOwner.SendAsync(new MsgSubPro
            {
                Action = AstProfAction.Activate,
                Class = type
            });
            return true;
        }

        public int GetPower(AstProfType type)
        {
            return AstProfManager.GetPower(type, AstProfManager.GetRank(mOwner, type));
        }

        public Task SendAsync()
        {
            MsgSubPro msg = new MsgSubPro();
            msg.Action = AstProfAction.ShowGui;
            msg.Points = mOwner.StudyPoints;
            foreach (var prof in mDictAstProfs.Values)
            {
                msg.Professions.Add(new AstProfStruct
                {
                    Class = prof.Class,
                    Level = prof.Level,
                    Rank = AstProfManager.GetRank(mOwner, prof.Class)
                });
            }
            return mOwner.SendAsync(msg);
        }

        public Task SendStudyAsync()
        {
            return mOwner.SendAsync(new MsgSubPro
            {
                Action = AstProfAction.UpdateStudy,
                Study = mOwner.StudyPoints
            });
        }
    }
}

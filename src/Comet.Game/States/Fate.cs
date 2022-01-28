using System.Collections.Generic;
using System.Threading.Tasks;
using Comet.Game.Database;
using Comet.Game.Database.Models;
using Comet.Game.Packets;
using Comet.Game.World.Managers;
using static Comet.Game.Packets.MsgTrainingVitality;

namespace Comet.Game.States
{
    public sealed class Fate
    {
        private readonly Character mOwner;
        private FateData mFate;
        private List<DbFateProtect> mProtect = new List<DbFateProtect>();

        public Fate(Character owner)
        {
            mOwner = owner;
        }

        public async Task InitializeAsync()
        {
            mFate = await FateManager.GetPlayerAsync(mOwner.Identity);
            if (mFate != null)
            {
                mProtect.AddRange(await DbFateProtect.GetAsync(mOwner.Identity));
                await SendAsync(true);
            }
        }

        public async Task UnlockAsync(FateType type)
        {
            var init = FateManager.GetInitializationRequirements(type);
            if (init == null)
                return;

            if (init.Data3 > mOwner.Metempsychosis)
                return;
            if (init.Data2 > mOwner.Level && init.Data3 >= mOwner.Metempsychosis)
                return;
            if (type > FateType.Dragon && IsLocked(type - 1))
                return;
            if (type > FateType.Dragon && init.Data4 > mOwner.Fate.GetScore(type - 1))
                return;
            if (!IsLocked(type))
                return;

            if (mFate == null)
            {
                mFate = new FateData(new DbFatePlayer
                {
                    PlayerId = mOwner.Identity
                })
                {
                    Name = mOwner.Name,
                    Lookface = mOwner.Mesh,
                    Level = mOwner.Level,
                    Mate = mOwner.MateName,
                    FirstProfession = mOwner.FirstProfession,
                    PreviousProfession = mOwner.PreviousProfession,
                    Profession = mOwner.Profession
                };
                FateManager.Add(mFate);

                mOwner.ChiPoints = 4000;
            }

            await FateManager.GenerateAsync(mOwner, type, mFate, TrainingSave.None);
            await mOwner.SendAsync(new MsgPlayerAttribInfo(mOwner));
        }

        public async Task GenerateAsync(FateType type, TrainingSave save)
        {
            if (IsLocked(type))
                return;

            int cost = 50;
            if (save.HasFlag(TrainingSave.Attr1))
                cost += 50;
            if (save.HasFlag(TrainingSave.Attr2))
                cost += 50;
            if (save.HasFlag(TrainingSave.Attr3))
                cost += 50;
            if (save.HasFlag(TrainingSave.Attr4))
                cost += 50;

            if (!await mOwner.SpendStrengthValueAsync(cost))
                return;

            await FateManager.GenerateAsync(mOwner, type, mFate, save);
            await mOwner.SendAsync(new MsgPlayerAttribInfo(mOwner));
        }

        public bool IsLocked(FateType type)
        {
            DbFatePlayer fate = mFate;
            if (fate == null)
                return true;

            if (type == FateType.Dragon)
            {
                return fate.Fate1Attrib1 == 0;
            }
            else if (type == FateType.Phoenix)
            {
                return fate.Fate2Attrib1 == 0;
            }
            else if (type == FateType.Tiger)
            {
                return fate.Fate3Attrib1 == 0;
            }
            else if (type == FateType.Turtle)
            {
                return fate.Fate4Attrib1 == 0;
            }
            return true;
        }

        public int GetScore(FateType type)
        {
            return FateManager.GetScore(mFate, type);
        }

        public int GetPower(TrainingAttrType attr)
        {
            return FateManager.GetPower(mFate, attr);
        }

        public Task SendAsync(bool update, Character target = null)
        {
            MsgTrainingVitalityInfo msg = new MsgTrainingVitalityInfo();
            msg.Mode = (ushort)(update ? 1 : 0);
            msg.Identity = mOwner.Identity;

            DbFatePlayer fate = mFate;
            if (fate != null)
            {
                if (fate.Fate1Attrib1 != 0)
                {
                    msg.Datas.Add(new MsgTrainingVitalityInfo.TrainingData
                    {
                        Type = FateType.Dragon,
                        Power1 = fate.Fate1Attrib1,
                        Power2 = fate.Fate1Attrib2,
                        Power3 = fate.Fate1Attrib3,
                        Power4 = fate.Fate1Attrib4
                    });
                }

                if (fate.Fate2Attrib1 != 0)
                {
                    msg.Datas.Add(new MsgTrainingVitalityInfo.TrainingData
                    {
                        Type = FateType.Phoenix,
                        Power1 = fate.Fate2Attrib1,
                        Power2 = fate.Fate2Attrib2,
                        Power3 = fate.Fate2Attrib3,
                        Power4 = fate.Fate2Attrib4
                    });
                }

                if (fate.Fate3Attrib1 != 0)
                {
                    msg.Datas.Add(new MsgTrainingVitalityInfo.TrainingData
                    {
                        Type = FateType.Tiger,
                        Power1 = fate.Fate3Attrib1,
                        Power2 = fate.Fate3Attrib2,
                        Power3 = fate.Fate3Attrib3,
                        Power4 = fate.Fate3Attrib4
                    });
                }

                if (fate.Fate4Attrib1 != 0)
                {
                    msg.Datas.Add(new MsgTrainingVitalityInfo.TrainingData
                    {
                        Type = FateType.Turtle,
                        Power1 = fate.Fate4Attrib1,
                        Power2 = fate.Fate4Attrib2,
                        Power3 = fate.Fate4Attrib3,
                        Power4 = fate.Fate4Attrib4
                    });
                }
            }

            if (target == null)
            {
                msg.Strength = mOwner.ChiPoints;
                msg.Data = fate?.AttribLockInfo ?? 0;
                return mOwner.SendAsync(msg);
            }
            return target.SendAsync(msg);
        }

        public Task<bool> SaveAsync()
        {
            DbFatePlayer fate = mFate;
            if (fate != null)
            {
                return BaseRepository.SaveAsync(fate);
            }
            return Task.FromResult(false);
        }

        public enum FateType
        {
            None,
            Dragon = 1,
            Phoenix,
            Tiger,
            Turtle
        }
    }
}

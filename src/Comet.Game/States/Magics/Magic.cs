// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) FTW! Masters
// Keep the headers and the patterns adopted by the project. If you changed anything in the file just insert
// your name below, but don't remove the names of who worked here before.
// 
// This project is a fork from Comet, a Conquer Online Server Emulator created by Spirited, which can be
// found here: https://gitlab.com/spirited/comet
// 
// Comet - Comet.Game - Magic.cs
// Description:
// 
// Creator: FELIPEVIEIRAVENDRAMI [FELIPE VIEIRA VENDRAMINI]
// 
// Developed by:
// Felipe Vieira Vendramini <felipevendramini@live.com>
// 
// Programming today is a race between software engineers striving to build bigger and better
// idiot-proof programs, and the Universe trying to produce bigger and better idiots.
// So far, the Universe is winning.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading.Tasks;
using Comet.Core;
using Comet.Game.Database;
using Comet.Game.Database.Models;
using Comet.Game.Packets;
using Comet.Game.States.BaseEntities;
using Comet.Game.World.Managers;
using Comet.Shared;

namespace Comet.Game.States.Magics
{
    public sealed class Magic
    {
        private Role m_pOwner;

        private DbMagic mMagic;
        private DbMagictype mMagicType;

        private TimeOutMS mDelay = new ();
        private TimeOutMS mWarningTime = new();
        private TimeOutMS mColdTime = new ();

        private byte m_pMaxLevel;

        public Magic(Role owner)
        {
            m_pOwner = owner;
        }

        public async Task<bool> CreateAsync(uint idMgc)
        {
            return await CreateAsync(idMgc, 0);
        }

        public async Task<bool> CreateAsync(uint idMgc, ushort level)
        {
            mMagicType = MagicManager.GetMagictype(idMgc, level);
            if (mMagicType == null)
            {
                await Log.WriteLogAsync(LogLevel.Warning, $"Skill not existent for creation (type:{idMgc}, level:{0}, player: {m_pOwner.Identity})");
                return false;
            }
            
            if (m_pOwner.MagicData.CheckType((ushort)idMgc))
                return false;
            
            mMagic = new DbMagic
            {
                OwnerId = m_pOwner.Identity,
                Type = (ushort)idMgc,
                Level = level
            };

            m_pMaxLevel = MagicManager.GetMaxLevel(idMgc);

            if (m_pOwner is Character)
            {
                await SaveAsync();
                await SendAsync();
            }
            return true;
        }

        public async Task<bool> CreateAsync(DbMagic pMgc)
        {
            mMagicType = MagicManager.GetMagictype(pMgc.Type, pMgc.Level);
            if (mMagicType == null)
            {
                await Log.WriteLogAsync(LogLevel.Warning, $"Skill not existent (type:{pMgc.Type}, level:{pMgc.Level}, player: {m_pOwner.Identity})");
                return false;
            }

            mMagic = pMgc;

            m_pMaxLevel = MagicManager.GetMaxLevel(pMgc.Type);
            return true;
        }

        #region Attributes

        public uint Identity => mMagic.Id;

        public string Name => mMagicType.Name;

        public ushort Type => mMagic.Type;

        public ushort Level
        {
            get => mMagic.Level;
            set
            {
                mMagic.Level = Math.Min(m_pMaxLevel, value);
                mMagicType = MagicManager.GetMagictype(Type, Level);
            }
        }

        public uint Experience
        {
            get => mMagic.Experience;
            set => mMagic.Experience = value;
        }

        public byte OldLevel
        {
            get => (byte)mMagic.OldLevel;
            set => mMagic.OldLevel = value;
        }

        public bool Unlearn
        {
            get => mMagic.Unlearn != 0;
            set => mMagic.Unlearn = (byte) (value ? 1 : 0);
        }

        public ushort MaxLevel => m_pMaxLevel;
        public MagicData.MagicSort Sort => (MagicData.MagicSort) mMagicType.Sort;
        public uint AutoActive => mMagicType.AutoActive;
        public byte Crime => mMagicType.Crime;
        public byte Ground => mMagicType.Ground;
        public byte Multi => mMagicType.Multi;
        public byte Target => (byte)mMagicType.Target;
        public uint UseMana => mMagicType.UseMp;
        public int Power => mMagicType.Power;
        public uint IntoneSpeed => mMagicType.IntoneSpeed;
        public uint Percent => mMagicType.Percent;
        public uint StepSeconds => mMagicType.StepSecs;
        public uint Range => mMagicType.Range;
        public uint Distance => mMagicType.Distance;
        public long Status => mMagicType.Status;
        public uint NeedProf => mMagicType.NeedProf;
        public int NeedExp => mMagicType.NeedExp;
        public uint NeedLevel => mMagicType.NeedLevel;
        public BattleSystem.MagicType UseXp => (BattleSystem.MagicType) mMagicType.UseXp;
        public uint WeaponSubtype => mMagicType.WeaponSubtype;
        public uint ActiveTimes => mMagicType.ActiveTimes;
        public uint FloorAttr => mMagicType.FloorAttr;
        public byte AutoLearn => mMagicType.AutoLearn;
        public byte DropWeapon => mMagicType.DropWeapon;
        public uint UseStamina => mMagicType.UseEp;
        public byte WeaponHit => mMagicType.WeaponHit;
        public uint UseItem => mMagicType.UseItem;
        public uint NextMagic => mMagicType.NextMagic;
        public int DelayMs => (int)(mMagicType.DelayMs > 0 ? mMagicType.DelayMs : mMagicType.Timeout);
        public uint UseItemNum => mMagicType.UseItemNum;
        public byte ElementType => mMagicType.ElementType;
        public uint ElementPower => mMagicType.ElementPower;
        public uint CpsCost => (uint)(mMagicType.ReqUplevTime / 22.22d);

        public uint ColdTime { get; set; }
        public uint WarningTime { get; set; }

        #endregion

        #region Checks

        public void SetDelay()
        {
            mDelay.Startup(DelayMs);
        }

        public void SetDelay(int delay)
        {
            mDelay.Startup(delay);
        }

        public bool Use()
        {
            if (!IsReady())
                return false;

            mDelay.Startup(DelayMs);
            if (ColdTime > 0)
                mColdTime.Startup((int)ColdTime);
            return true;
        }

        public bool IsReady()
        {
            if (!mDelay.IsActive())
            {
                mDelay.Startup(DelayMs);
                return true;
            }
            return mDelay.IsTimeOut(DelayMs);
        }

        #endregion

        #region AI

        public bool IsCold()
        {
            return mColdTime.IsTimeOut();
        }

        public void Warn()
        {
            mWarningTime.Startup((int)WarningTime);
        }

        public bool IsWarned()
        {
            return mWarningTime.IsActive();
        }

        public bool WarningTimeOut()
        {
            return mWarningTime.IsTimeOut();
        }

        public void ClearWarning()
        {
            mWarningTime.Clear();
        }

        #endregion

        #region Socket

        public async Task SendAsync()
        {
            if (m_pOwner is Character user)
                await user.SendAsync(new MsgMagicInfo
                {
                    Experience = Experience,
                    Level = Level,
                    Magictype = Type
                });
        }

        public async Task FlushAsync()
        {
            if (m_pOwner is Character user)
                await user.SendAsync(new MsgFlushExp
                {
                    Action = MsgFlushExp.FlushMode.Magic,
                    Identity = Type,
                    Experience = Experience
                });
        }

        #endregion

        #region Database

        public async Task<bool> SaveAsync()
        {
            try
            {
                await using ServerDbContext context = new ServerDbContext();
                if (mMagic.Id == 0)
                    context.Magic.Add(mMagic);
                else context.Magic.Update(mMagic);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await Log.WriteLogAsync(LogLevel.Exception, ex.ToString());
                return false;
            }
        }

        public async Task<bool> DeleteAsync()
        {
            try
            {
                mMagic.Unlearn = 1;
                return await SaveAsync();
            }
            catch (Exception ex)
            {
                await Log.WriteLogAsync(LogLevel.Exception, ex.ToString());
                return false;
            }
        }

        #endregion
    }
}
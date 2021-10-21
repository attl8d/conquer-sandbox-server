// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) FTW! Masters
// Keep the headers and the patterns adopted by the project. If you changed anything in the file just insert
// your name below, but don't remove the names of who worked here before.
// 
// This project is a fork from Comet, a Conquer Online Server Emulator created by Spirited, which can be
// found here: https://gitlab.com/spirited/comet
// 
// Comet - Comet.Game - MsgPlayer.cs
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

#define NO_COLOR

#region References

using System;
using System.Collections.Generic;
using System.IO;
using Comet.Game.States;
using Comet.Game.States.Items;
using Comet.Network.Packets;

#endregion

namespace Comet.Game.Packets
{
    public sealed class MsgPlayer : MsgBase<Client>
    {
        public MsgPlayer(Character user)
        {
            Type = PacketType.MsgPlayer;

            Identity = user.Identity;
            Mesh = user.Mesh;

            MapX = user.MapX;
            MapY = user.MapY;

            Status = (uint) user.StatusFlag;

            Hairstyle = user.Hairstyle;
            Direction = (byte) user.Direction;
            Pose = (byte) user.Action;
            Metempsychosis = user.Metempsychosis;
            Level = user.Level;

            SyndicateIdentity = user.SyndicateIdentity;
            SyndicatePosition = (ushort) user.SyndicateRank;

            NobilityRank = (uint) user.NobilityRank;

            Helmet = user.Headgear?.Type ?? 0;
            HelmetColor = (ushort) (user.Headgear?.Color ?? Item.ItemColor.None);
            RightHand = user.RightHand?.Type ?? 0;
            LeftHand = user.LeftHand?.Type ?? 0;
            LeftHandColor = (ushort) (user.LeftHand?.Color ?? Item.ItemColor.None);
            Armor = user.Armor?.Type ?? 0;
            ArmorColor = (ushort) (user.Armor?.Color ?? Item.ItemColor.None);
            Garment = user.Garment?.Type ?? 0;

            FlowerRanking = user.FlowerCharm;
            QuizPoints = user.QuizPoints;
            UserTitle = user.UserTitle;

            Away = user.IsAway;
            
            if (user.Syndicate != null)
                SharedBattlePower = (uint) user.Syndicate.GetSharedBattlePower(user.SyndicateRank);

            FamilyIdentity = user.FamilyIdentity;
            FamilyRank = (uint) user.FamilyPosition;
            FamilyBattlePower = user.FamilyBattlePower;

            UserTitle = user.UserTitle;
            TotemBattlePower = user.TotemBattlePower;

            CurrentProfession = user.Profession;
            FirstProfession = user.FirstProfession;
            LastProfession = user.PreviousProfession;

            BattlePower = user.BattlePower;

            NationalityFlag = (int) user.PlayerCountry;

            Name = user.Name;
            FamilyName = user.FamilyName;
        }

        public MsgPlayer(Monster monster)
        {
            Type = PacketType.MsgPlayer;

            Identity = monster.Identity;

            Mesh = monster.Mesh;

            MapX = monster.MapX;
            MapY = monster.MapY;

            Status = (uint) monster.StatusFlag;

            Direction = (byte) monster.Direction;
            Pose = (byte) monster.Action;

            SpeciesType = monster.SpeciesType;
            MonsterLevel = monster.Level;
            if (monster.SpeciesType == 1)
            {
                MonsterLife = (ushort)(ushort.MaxValue / (double)monster.Life * 100);
            }
            else
            {
                MonsterLife = monster.Life;
            }

            Name = monster.Name;
        }

        public uint Identity { get; set; }
        public uint Mesh { get; set; }

        #region Union

        #region Struct

        public uint SyndicateIdentity { get; set; }
        public uint SyndicatePosition { get; set; }

        #endregion

        public uint OwnerIdentity { get; set; }

        #endregion

        #region Union

        public ulong Status { get; set; }

        #region Struct

        public ushort StatuaryLife { get; set; }
        public ushort StatuaryFrame { get; set; }

        #endregion

        #endregion

        public ulong Status2 { get; set; }
        public uint Status3 { get; set; }

        public ushort CurrentLayout { get; set; }
       
        public uint Garment { get; set; }
        public uint Helmet { get; set; }
        public uint Armor { get; set; }
        public uint RightHand { get; set; }
        public uint LeftHand { get; set; }
        public uint Mount { get; set; }
        public uint MountArmor { get; set; }
        public uint RightAccessory { get; set; }
        public uint LeftAccessory { get; set; }

        public uint MonsterLife { get; set; }
        public ushort MonsterLevel { get; set; }

        public ushort MapX { get; set; }
        public ushort MapY { get; set; }
        public ushort Hairstyle { get; set; }
        public byte Direction { get; set; }
        public byte Pose { get; set; }
        public ushort Metempsychosis { get; set; }
        public ushort Level { get; set; }
        public bool WindowSpawn { get; set; }
        public bool Away { get; set; }
        public uint SharedBattlePower { get; set; }
        public uint FlowerRanking { get; set; }

        public uint NobilityRank { get; set; }

        public ushort Padding2 { get; set; }

        public ushort HelmetColor { get; set; }
        public ushort ArmorColor { get; set; }
        public ushort LeftHandColor { get; set; }
        public uint QuizPoints { get; set; }

        public byte MountAddition { get; set; }
        public uint MountColor { get; set; }
        public ushort EnlightenPoints { get; set; }

        public byte SpeciesType
        {
            get => (byte) FamilyIdentity;
            set => FamilyIdentity = value;
        }

        public uint FamilyIdentity { get; set; }
        public uint FamilyRank { get; set; }
        public int FamilyBattlePower { get; set; }

        public uint UserTitle { get; set; }
        public int TotemBattlePower { get; set; }
        public int IsArenaWitness { get; set; }

        public uint HelmetArtifact { get; set; }
        public uint ArmorArtifact { get; set; }
        public uint LeftHandArtifact { get; set; }
        public uint RightHandArtifact { get; set; }

        public byte CurrentAstProf { get; set; }
        public ushort FirstProfession { get; set; }
        public ushort LastProfession { get; set; }
        public ushort CurrentProfession { get; set; }

        public int NationalityFlag { get; set; }
        public int BattlePower { get; set; }
        
        public byte TalentPoints { get; set; }
        public bool KongFuActive { get; set; }

        public byte SkillSoul { get; set; }

        public string Name { get; set; }
        public string FamilyName { get; set; }

        /// <summary>
        ///     Encodes the packet structure defined by this message class into a byte packet
        ///     that can be sent to the client. Invoked automatically by the client's send
        ///     method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            var writer = new PacketWriter();
            writer.Write((ushort) Type);
            writer.Write(0); // 4
            writer.Write(Mesh); // 8
            writer.Write(Identity); // 12

            if (OwnerIdentity > 0)
            {
                writer.Write(OwnerIdentity); // 16
                writer.Write(0); // 20
            }
            else
            {
                writer.Write(SyndicateIdentity); // 16
                writer.Write(SyndicatePosition); // 20
            }

            writer.Write((ushort)0); // 24

            if (StatuaryLife > 0)
            {
                writer.Write(StatuaryLife); // 26
                writer.Write(StatuaryFrame); // 28
                writer.Write(0u); // 30
            }
            else
            {
                writer.Write(Status); // 26
            }

            writer.Write(Status2); // 34
            writer.Write(Status3); // 42

            writer.Write(CurrentLayout); // 46

            writer.Write(Helmet); // 48
            writer.Write(Garment); // 52
            writer.Write(Armor); // 56
            writer.Write(LeftHand); // 60
            writer.Write(RightHand); // 64
            writer.Write(LeftAccessory); // 68
            writer.Write(RightAccessory); // 72
            writer.Write(Mount); // 76
            writer.Write(MountArmor); // 80
            writer.BaseStream.Seek(6, SeekOrigin.Current); // 84
            writer.Write(MonsterLife); // 90
            writer.Write((ushort) 0); // 92
            writer.Write(MonsterLevel); // 96
            writer.Write(MapX); // 98
            writer.Write(MapY); // 100
            writer.Write(Hairstyle); // 102
            writer.Write(Direction); // 104
            writer.Write(Pose); // 105
            writer.BaseStream.Seek(6, SeekOrigin.Current); // 106
            writer.Write((byte) Metempsychosis); // 112
            writer.Write(Level); // 113
            writer.Write(WindowSpawn); // 115
            writer.Write(Away); // 116
            writer.Write(SharedBattlePower); // 117
            writer.BaseStream.Seek(12, SeekOrigin.Current); // 121
            writer.Write(FlowerRanking); // 133
            writer.Write(NobilityRank); // 137
            writer.Write(ArmorColor); // 141
            writer.Write(LeftHandColor); // 143
            writer.Write(HelmetColor); // 145
            writer.Write(QuizPoints); // 147
            writer.Write(MountAddition); // 151
            writer.BaseStream.Seek(5, SeekOrigin.Current); // 152
            writer.Write(MountColor); // 157
            writer.Write((byte) 0); // 161
            writer.Write(EnlightenPoints); // 162
            writer.BaseStream.Seek(9, SeekOrigin.Current); // 164
            writer.Write(FamilyIdentity); // 173
            writer.Write(FamilyRank); // 177
            writer.Write(FamilyBattlePower); // 181
            writer.Write(UserTitle); // 185
            writer.BaseStream.Seek(3, SeekOrigin.Current); // 189
            writer.Write(TotemBattlePower); // 192
            writer.Write(IsArenaWitness); // 196
            writer.Write(HelmetArtifact); // 200
            writer.Write(ArmorArtifact); // 204
            writer.Write(LeftHandArtifact); // 208
            writer.Write(RightHandArtifact); // 212
            writer.BaseStream.Seek(8, SeekOrigin.Current); // 216
            writer.Write(CurrentAstProf); // 224
            writer.Write(FirstProfession); // 225
            writer.Write(LastProfession); // 227
            writer.Write(CurrentProfession); // 229
            writer.Write(NationalityFlag); // 231
            writer.Write((ushort)0); // 235
            writer.Write(BattlePower); // 237
            writer.Write(TalentPoints); // 2417
            writer.Write(KongFuActive); // 242
            writer.Write(SkillSoul); // 243
            if (!string.IsNullOrEmpty(FamilyName))
            {
                writer.Write(new List<string> // 244
                {
                    Name,
                    Language.StrNone,
                    FamilyName
                });
            }
            else
            {
                writer.Write(new List<string>
                {
                    Name
                });
            }
            return writer.ToArray();
        }
    }
}
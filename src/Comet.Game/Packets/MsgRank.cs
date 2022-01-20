// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) FTW! Masters
// Keep the headers and the patterns adopted by the project. If you changed anything in the file just insert
// your name below, but don't remove the names of who worked here before.
// 
// This project is a fork from Comet, a Conquer Online Server Emulator created by Spirited, which can be
// found here: https://gitlab.com/spirited/comet
// 
// Comet - Comet.Game - MsgRank.cs
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

#region References

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Comet.Game.Database.Models;
using Comet.Game.States;
using Comet.Game.World.Managers;
using Comet.Network.Packets;
using Comet.Shared;

#endregion

namespace Comet.Game.Packets
{
    public sealed class MsgRank : MsgBase<Client>
    {
        public enum RequestType
        {
            None,
            RequestRank,
            QueryInfo,
            QueryIcon = 5
        }

        public enum RankType : byte
        {
            Flower,
            ChiDragon,
            ChiPhoenix,
            ChiTiger,
            ChiTurtle
        }

        public RequestType Mode { get; set; }
        public uint Identity { get; set; }
        public RankType RankMode { get; set; }
        public byte Subtype { get; set; }
        public ushort Data1 { get; set; }
        public ushort Data2 { get; set; }
        public ushort PageNumber { get; set; }

        public List<string> Strings { get; set; } = new List<string>();
        public List<QueryStruct> Infos { get; set; } = new List<QueryStruct>();
        public List<RankData> Rank { get; set; } = new List<RankData> { };

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16(); // 0
            Type = (PacketType)reader.ReadUInt16(); // 2
            Mode = (RequestType)reader.ReadUInt32(); // 4
            Identity = reader.ReadUInt32(); // 8
            RankMode = (RankType)reader.ReadByte(); // 12
            Subtype = reader.ReadByte(); // 13
            PageNumber = reader.ReadUInt16(); // 14
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgRank);
            writer.Write((uint)Mode); // 4
            writer.Write(Identity); // 8
            writer.Write((byte)RankMode); // 12
            writer.Write(Subtype); // 13
            writer.Write(PageNumber); // 14
            if (Mode == RequestType.QueryInfo || Mode == RequestType.QueryIcon)
            {
                writer.Write(Infos.Count); // 16
                writer.Write(0); // 20
                foreach (var info in Infos)
                {
                    writer.Write((long)info.Position); // 24 0
                    writer.Write((long)info.Amount); // 32 8
                    writer.Write(info.Identity); // 40 16
                    writer.Write(info.Identity); // 44 20
                    writer.Write(info.Name, 16); // 48 24
                    writer.Write(info.Name, 16); // 64 40
                }
            }
            else if (Mode == RequestType.RequestRank)
            {
                writer.Write(Rank.Count); // 16
                writer.Write(0); // 20
                foreach (var rank in Rank)
                {
                    writer.Write(rank.Position); // 0
                    writer.Write(0); // 4
                    writer.Write(rank.Amount); // 8
                    writer.Write(rank.Identity); // 16
                    writer.Write(rank.Identity); // 20
                    writer.Write(rank.Name, 16); // 24
                    writer.Write(rank.Name, 16); // 40
                }
            }
            else
            {
                writer.Write(1);
                writer.Write(0);
            }
            return writer.ToArray();
        }

        public override async Task ProcessAsync(Client client)
        {
            Character user = client.Character;
            switch (Mode)
            {
                case RequestType.RequestRank:
                    {
                        if (RankMode == RankType.Flower)
                        {
                            List<FlowerManager.FlowerRankingStruct> ranking = default;
                            if (Identity == 0x1c9c382) // Red Rose
                            {
                                ranking = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.RedRose, 0, 100);
                            }
                            else if (Identity == 0x1c9c3e6) // White Rose
                            {
                                ranking = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.WhiteRose, 0, 100);
                            }
                            else if (Identity == 0x1c9c44a) // Orhcid 
                            {
                                ranking = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.Orchid, 0, 100);
                            }
                            else if (Identity == 0x1c9c4ae) // Tulip
                            {
                                ranking = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.Tulip, 0, 100);
                            }
                            else return;

                            int position = ranking.FirstOrDefault(x => x.Identity == user.Identity).Position;
                            if (position <= 0 || position > 100)
                                return;

                            int index = PageNumber * 10;
                            int count = 0;
                            if (index >= ranking.Count)
                                return;

                            for (; index < ranking.Count && count < 10; index++, count++)
                            {
                                Rank.Add(new RankData
                                {
                                    Identity = ranking[index].Identity,
                                    Position = index + 1,
                                    Amount = ranking[index].Value,
                                    Name = ranking[index].Name,
                                });
                            }

                            await user.SendAsync(this);
                        }
                        break;
                    }

                case RequestType.QueryInfo:
                    {
                        FlowerManager.FlowerRankObject flowerToday = await FlowerManager.QueryFlowersAsync(user);
                        await user.SendAsync(new MsgFlower
                        {
                            Mode = MsgFlower.RequestMode.QueryFlower,
                            Identity = user.Identity,
                            RedRoses = user.FlowerRed,
                            RedRosesToday = flowerToday?.RedRose ?? 0,
                            WhiteRoses = user.FlowerWhite,
                            WhiteRosesToday = flowerToday?.WhiteRose ?? 0,
                            Orchids = user.FlowerOrchid,
                            OrchidsToday = flowerToday?.Orchids ?? 0,
                            Tulips = user.FlowerTulip,
                            TulipsToday = flowerToday?.Tulips ?? 0
                        });

                        if (user.CanRefreshFlowerRank)
                        {
                            var roseRank = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.RedRose, 0, 100);
                            var lilyRank = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.WhiteRose, 0, 100);
                            var orchidRank = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.Orchid, 0, 100);
                            var tulipRank = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.Tulip, 0, 100);

                            var roseRankToday = FlowerManager.GetFlowerRankingToday(MsgFlower.FlowerType.RedRose, 0, 100);
                            var lilyRankToday = FlowerManager.GetFlowerRankingToday(MsgFlower.FlowerType.WhiteRose, 0, 100);
                            var orchidRankToday = FlowerManager.GetFlowerRankingToday(MsgFlower.FlowerType.Orchid, 0, 100);
                            var tulipRankToday = FlowerManager.GetFlowerRankingToday(MsgFlower.FlowerType.Tulip, 0, 100);

                            int myRose = roseRank.FirstOrDefault(x => x.Identity == user.Identity).Position;
                            int myLily = lilyRank.FirstOrDefault(x => x.Identity == user.Identity).Position;
                            int myOrchid = orchidRank.FirstOrDefault(x => x.Identity == user.Identity).Position;
                            int myTulip = tulipRank.FirstOrDefault(x => x.Identity == user.Identity).Position;

                            int myRoseToday = roseRankToday.FirstOrDefault(x => x.Identity == user.Identity).Position;
                            int myLilyToday = lilyRankToday.FirstOrDefault(x => x.Identity == user.Identity).Position;
                            int myOrchidToday = orchidRankToday.FirstOrDefault(x => x.Identity == user.Identity).Position;
                            int myTulipToday = tulipRankToday.FirstOrDefault(x => x.Identity == user.Identity).Position;

                            uint rankType = 0;
                            uint amount = 0;
                            int rank = 0;

                            bool display = false;
                            if (myRoseToday < myRose && myRoseToday > 0 && myRoseToday <= 100)
                            {
                                rankType = 0x1c9c382;
                                amount = flowerToday?.RedRose ?? 0;
                                rank = myRoseToday;
                                display = true;
                            }
                            else if (myRose > 0 && myRose <= 100)
                            {
                                rankType = 0x1c9c382;
                                amount = user.FlowerRed;
                                rank = myRose;
                                display = true;
                            }

                            MsgRank msg;
                            if (display)
                            {
                                msg = new MsgRank();
                                msg.Mode = RequestType.QueryInfo;
                                msg.Identity = rankType;
                                msg.Infos.Add(new QueryStruct
                                {
                                    Type = 1,
                                    Amount = amount,
                                    Identity = user.Identity,
                                    Name = user.Name,
                                    Position = rank
                                });
                                await user.SendAsync(msg);
                            }

                            display = false;
                            if (myLilyToday < myLily && myLilyToday > 0 && myLilyToday <= 100)
                            {
                                rankType = 0x1c9c3e6;
                                amount = flowerToday?.WhiteRose ?? 0;
                                rank = myLilyToday;
                                display = true;
                            }
                            else if (myLily > 0 && myLily <= 100)
                            {
                                rankType = 0x1c9c3e6;
                                amount = user.FlowerWhite;
                                rank = myLily;
                                display = true;
                            }

                            if (display)
                            {
                                msg = new MsgRank();
                                msg.Mode = RequestType.QueryInfo;
                                msg.Identity = rankType;
                                msg.Infos.Add(new QueryStruct
                                {
                                    Type = 1,
                                    Amount = amount,
                                    Identity = user.Identity,
                                    Name = user.Name,
                                    Position = rank
                                });
                                await user.SendAsync(msg);
                            }

                            display = false;
                            if (myOrchidToday < myOrchid && myOrchidToday > 0 && myOrchidToday <= 100)
                            {
                                rankType = 0x1c9c44a;
                                amount = flowerToday?.Orchids ?? 0;
                                rank = myOrchidToday;
                                display = true;
                            }
                            else if (myOrchid > 0 && myOrchid <= 100)
                            {
                                rankType = 0x1c9c44a;
                                amount = user.FlowerOrchid;
                                rank = myOrchid;
                                display = true;
                            }

                            if (display)
                            {
                                msg = new MsgRank();
                                msg.Mode = RequestType.QueryInfo;
                                msg.Identity = rankType;
                                msg.Infos.Add(new QueryStruct
                                {
                                    Type = 1,
                                    Amount = amount,
                                    Identity = user.Identity,
                                    Name = user.Name,
                                    Position = rank
                                });
                                await user.SendAsync(msg);
                            }

                            display = false;
                            if (myTulipToday < myTulip && myTulipToday > 0 && myTulipToday <= 100)
                            {
                                rankType = 0x1c9c4ae;
                                amount = flowerToday?.Tulips ?? 0;
                                rank = myTulipToday;
                                display = true;
                            }
                            else if (myTulip > 0 && myTulip <= 100)
                            {
                                rankType = 0x1c9c4ae;
                                amount = user.FlowerTulip;
                                rank = myTulip;
                                display = true;
                            }

                            if (display)
                            {
                                msg = new MsgRank();
                                msg.Mode = RequestType.QueryInfo;
                                msg.Identity = rankType;
                                msg.Infos.Add(new QueryStruct
                                {
                                    Type = 1,
                                    Amount = amount,
                                    Identity = user.Identity,
                                    Name = user.Name,
                                    Position = rank
                                });
                                await user.SendAsync(msg);
                            }

                            if (rankType != user.FlowerCharm)
                            {
                                user.FlowerCharm = rankType;
                                await user.Screen.SynchroScreenAsync();
                            }

                            Mode = RequestType.QueryIcon;
                            await client.SendAsync(this);
                        }
                        break;
                    }
                default:
                    {
                        await Log.WriteLogAsync(LogLevel.Error, $"Unhandled MsgRank:{Mode}");
                        await Log.WriteLogAsync(LogLevel.Debug, PacketDump.Hex(Encode()));
                        return;
                    }
            }
        }

        public struct QueryStruct
        {
            public int Position;
            public ulong Type;
            public ulong Amount;
            public uint Identity;
            public string Name;
        }

        public struct RankData
        {
            public int Position;
            public long Amount;
            public uint Identity { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public int Profession { get; set; }
            public uint LookFace { get; set; }
        }
    }
}
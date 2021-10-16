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
            if (Mode == RequestType.QueryInfo)
            {
                writer.Write(Infos.Count); // 16
                writer.Write(0); // 20
                //foreach (var info in Infos)
                //{
                //    writer.Write(info.Position); // 24 0
                //    writer.Write(0); // 28 4
                //    writer.Write(info.Amount); // 32 8
                //    writer.Write(0); // 36 12
                //    writer.Write(info.Identity); // 40 16
                //    writer.Write(info.Identity); // 44 20
                //    writer.Write(info.Name, 16); // 48 36
                //    writer.Write(info.Name, 16); // 64 52
                //    writer.Write(0); // 68
                //    writer.Write(0); // 72 
                //    writer.Write(0); // 76
                //    writer.Write(0); // 80
                //}
                writer.Write(1); // 24
                writer.Write(0); // 28
                writer.Write(1); // 32
                writer.Write(0); // 36
                var info = Infos.First();
                writer.Write(info.Identity); // 40
                writer.Write(info.Identity); // 44
                writer.Write(info.Name, 16);
                string output = $"{info.Type} {info.Amount} {info.Identity} {info.Identity} {info.Name} {info.Name}";
                writer.Write(output, output.Length); // 64
            }
            else if (Mode == RequestType.RequestRank)
            {
                writer.Write(Rank.Count); // 16
                writer.Write(0); // 20
                foreach (var rank in Rank)
                {
                    writer.Write(rank.Position); // 24
                    writer.Write(0); // 28
                    writer.Write(rank.Amount); // 32
                    writer.Write(0); // 36
                    writer.Write(rank.Identity); // 40
                    writer.Write(rank.Identity); // 44
                    writer.Write(rank.Name, 16); // 48
                    writer.Write(rank.Name, 16); // 64
                    writer.Write(rank.Level); // 68
                    writer.Write(rank.Profession); // 72
                    writer.Write(rank.LookFace); // 76
                    writer.Write(0); // 80
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
                case RequestType.QueryInfo:
                    {
                        if (user.Gender != 2)
                            return;

                        FlowerManager.FlowerRankObject flowerToday = await Kernel.FlowerManager.QueryFlowersAsync(user);
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

                        await user.SendAsync(new MsgRank
                        {
                            Mode = RequestType.QueryIcon
                        }); 

                        if (user.CanRefreshFlowerRank)
                        {
                            var roseRank = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.RedRose, 0, 100);
                            var lilyRank = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.WhiteRose, 0, 100);
                            var orchidRank = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.Orchid, 0, 100);
                            var tulipRank = await FlowerManager.GetFlowerRankingAsync(MsgFlower.FlowerType.Tulip, 0, 100);

                            var roseRankToday = Kernel.FlowerManager.GetFlowerRankingToday(MsgFlower.FlowerType.RedRose, 0, 100);
                            var lilyRankToday = Kernel.FlowerManager.GetFlowerRankingToday(MsgFlower.FlowerType.WhiteRose, 0, 100);
                            var orchidRankToday = Kernel.FlowerManager.GetFlowerRankingToday(MsgFlower.FlowerType.Orchid, 0, 100);
                            var tulipRankToday = Kernel.FlowerManager.GetFlowerRankingToday(MsgFlower.FlowerType.Tulip, 0, 100);

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
                            if (myRoseToday < myRose && myRoseToday > 0 && myRoseToday <= 100)
                            {
                                // today rank
                                if (myRoseToday < 3)
                                    rankType = 30010001;
                                else if (myRoseToday < 10)
                                    rankType = 30020001;
                                else if (myRoseToday < 25)
                                    rankType = 30040001;
                                else
                                    rankType = 30110001;
                                amount = flowerToday?.RedRose ?? 0;
                                rank = myRoseToday;
                            }
                            else if (myRose > 0 && myRose <= 100)
                            {
                                // lifetime
                                if (myRose < 3)
                                    rankType = 30010002;
                                else if (myRose < 10)
                                    rankType = 30020002;
                                else if (myRose < 25)
                                    rankType = 30040002;
                                else
                                    rankType = 30110002;
                                amount = user.FlowerRed;
                                rank = myRose;
                            }

                            if (myLilyToday < myLily && myLilyToday > 0 && myLilyToday <= 100)
                            {
                                // today rank
                                if (myLilyToday < 3)
                                    rankType = 30010101;
                                else if (myLilyToday < 10)
                                    rankType = 30020101;
                                else if (myLilyToday < 25)
                                    rankType = 30040101;
                                else
                                    rankType = 30110101;
                                amount = flowerToday?.WhiteRose ?? 0;
                                rank = myLilyToday;
                            }
                            else if (myLily > 0 && myLily <= 100)
                            {
                                // lifetime
                                if (myLily < 3)
                                    rankType = 30010102;
                                else if (myLily < 10)
                                    rankType = 30020102;
                                else if (myLily < 25)
                                    rankType = 30040102;
                                else
                                    rankType = 30110102;
                                amount = user.FlowerWhite;
                                rank = myLily;
                            }

                            if (myOrchidToday < myOrchid && myOrchidToday > 0 && myOrchidToday <= 100)
                            {
                                // today rank
                                if (myOrchidToday < 3)
                                    rankType = 30010301;
                                else if (myOrchidToday < 10)
                                    rankType = 30020301;
                                else if (myOrchidToday < 25)
                                    rankType = 30040301;
                                else
                                    rankType = 30110301;
                                amount = flowerToday?.Orchids ?? 0;
                                rank = myOrchidToday;
                            }
                            else if (myOrchid > 0 && myOrchid <= 100)
                            {
                                // lifetime
                                if (myOrchid < 3)
                                    rankType = 30010302;
                                else if (myOrchid < 10)
                                    rankType = 30020302;
                                else if (myOrchid < 25)
                                    rankType = 30040302;
                                else
                                    rankType = 30110302;
                                amount = user.FlowerOrchid;
                                rank = myOrchid;
                            }

                            if (myTulipToday < myTulip && myTulipToday > 0 && myTulipToday <= 100)
                            {
                                // today rank
                                if (myTulipToday < 3)
                                    rankType = 30010401;
                                else if (myTulipToday < 10)
                                    rankType = 30020401;
                                else if (myTulipToday < 25)
                                    rankType = 30040401;
                                else
                                    rankType = 30110401;
                                amount = flowerToday?.Tulips ?? 0;
                                rank = myTulipToday;
                            }
                            else if (myTulip > 0 && myTulip <= 100)
                            {
                                // lifetime
                                if (myTulip < 3)
                                    rankType = 30010402;
                                else if (myTulip < 10)
                                    rankType = 30020402;
                                else if (myTulip < 25)
                                    rankType = 30040402;
                                else
                                    rankType = 30110402;
                                amount = user.FlowerTulip;
                                rank = myTulip;
                            }

                            if (rankType != user.FlowerCharm)
                            {
                                user.FlowerCharm = rankType;
                                await user.Screen.SynchroScreenAsync();
                            }

                            MsgRank msg = new MsgRank();
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
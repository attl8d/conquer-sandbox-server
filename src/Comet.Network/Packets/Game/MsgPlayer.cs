using System.Collections.Generic;
using System.IO;

namespace Comet.Network.Packets.Game
{
    public abstract class MsgPlayer<T> : MsgBase<T>
    {
        public uint Identity { get; set; }
        public uint Mesh { get; set; }


        public uint Garment { get; set; }
        public uint Helmet { get; set; }
        public uint Armor { get; set; }
        public uint RightHand { get; set; }
        public uint LeftHand { get; set; }
        public uint Mount { get; set; }

        public uint Padding0 { get; set; }

        public ushort MonsterLife { get; set; }
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
        public ushort CanBeEnlightened { get; set; }

        public uint FamilyIdentity { get; set; }
        public uint FamilyRank { get; set; }
        public int FamilyBattlePower { get; set; }

        public uint UserTitle { get; set; }

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
            writer.Write((ushort) PacketType.MsgPlayer);
            writer.Write(Mesh);     // 4
            writer.Write(Identity); // 8

            if (OwnerIdentity > 0)
            {
                writer.Write(OwnerIdentity); // 12
                writer.Write(0);             // 16
            }
            else
            {
                writer.Write(SyndicateIdentity); // 12
                writer.Write(SyndicatePosition); // 16
            }

            writer.Write((ushort) 0);

            if (StatuaryLife > 0)
            {
                writer.Write(StatuaryLife);  // 24
                writer.Write(StatuaryFrame); // 26
                writer.Write(0u);            // 28
            }
            else
            {
                writer.Write(Status); // 24
            }

            writer.Write(Helmet);       // 30
            writer.Write(Garment);      // 34
            writer.Write(Armor);        // 38
            writer.Write(RightHand);    // 42
            writer.Write(LeftHand);     // 46
            writer.Write(Mount);        // 50
            writer.Write(Padding0);     // 54
            writer.Write(MonsterLife);  // 58
            writer.Write(MonsterLevel); // 60
            writer.Write(Hairstyle);    // 62
            writer.Write(MapX);         // 64
            writer.Write(MapY);         // 66
            writer.Write(Direction);    // 68
            writer.Write(Pose);         // 69
            writer.BaseStream.Seek(4, SeekOrigin.Current);
            writer.Write((byte) Metempsychosis);           // 73
            writer.Write(Level);                           // 74
            writer.Write(WindowSpawn);                     // 76
            writer.Write(Away);                            // 77
            writer.Write(SharedBattlePower);               // 78
            writer.BaseStream.Seek(8, SeekOrigin.Current); // 82
            writer.Write(FlowerRanking);                   // 91
            writer.Write(NobilityRank);                    // 95
            writer.Write(ArmorColor);                      // 99
            writer.Write(LeftHandColor);                   // 101
            writer.Write(HelmetColor);                     // 103
            writer.Write(QuizPoints);                      // 105
            writer.Write(MountAddition);                   // 107
            writer.Write(0);                               // 108
            writer.Write(MountColor);                      // 112
            writer.Write((byte) 0);                        // 116
            writer.Write(EnlightenPoints);                 // 117
            //writer.BaseStream.Seek(10, SeekOrigin.Current); // 119
            writer.Write(CanBeEnlightened);                // 119
            writer.Write(0);                               // 121
            writer.Write(0);                               // 125
            writer.Write(FamilyIdentity);                  // 129
            writer.Write(FamilyRank);                      // 133
            writer.Write(FamilyBattlePower);               // 137
            writer.Write(UserTitle);                       // 141
            writer.BaseStream.Seek(8, SeekOrigin.Current); // 145
            writer.Write(new List<string>                  // 95
            {
                Name,
                FamilyName
            });


            return writer.ToArray();
        }

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
    }
}
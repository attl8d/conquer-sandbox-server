// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) FTW! Masters
// Keep the headers and the patterns adopted by the project. If you changed anything in the file just insert
// your name below, but don't remove the names of who worked here before.
// 
// This project is a fork from Comet, a Conquer Online Server Emulator created by Spirited, which can be
// found here: https://gitlab.com/spirited/comet
// 
// Comet - Comet.Game - MsgAllot.cs
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

using System.Threading.Tasks;
using Comet.Game.States;
using Comet.Network.Packets;

#endregion

namespace Comet.Game.Packets
{
    public sealed class MsgAllot : MsgBase<Client>
    {
        public MsgAllot()
        {
            Type = PacketType.MsgAllot;
        }

        public int Identity { get; set; } // ??
        public int Force { get; set; }
        public int Speed { get; set; }
        public int Health { get; set; }
        public int Soul { get; set; }

        /// <summary>
        ///     Decodes a byte packet into the packet structure defined by this message class.
        ///     Should be invoked to structure data from the client for processing. Decoding
        ///     follows TQ Digital's byte ordering rules for an all-binary protocol.
        /// </summary>
        /// <param name="bytes">Bytes from the packet processor or client socket</param>
        public override void Decode(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Identity = reader.ReadInt32();
            Force = reader.ReadInt32();
            Speed = reader.ReadInt32();
            Health = reader.ReadInt32();
            Soul = reader.ReadInt32();
        }

        /// <summary>
        ///     Encodes the packet structure defined by this message class into a byte packet
        ///     that can be sent to the client. Invoked automatically by the client's send
        ///     method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            var writer = new PacketWriter();
            writer.Write((ushort)Type);
            writer.Write(Force);
            writer.Write(Speed);
            writer.Write(Health);
            writer.Write(Soul);
            return writer.ToArray();
        }

        /// <summary>
        ///     Process can be invoked by a packet after decode has been called to structure
        ///     packet fields and properties. For the server implementations, this is called
        ///     in the packet handler after the message has been dequeued from the server's
        ///     <see cref="PacketProcessor{TClient}" />.
        /// </summary>
        /// <param name="client">Client requesting packet processing</param>
        public override async Task ProcessAsync(Client client)
        {
            Character user = client.Character;

            if (Force < 0 || Speed < 0 || Health < 0 || Soul < 0)
                return;

            if (Force > short.MaxValue || Speed > short.MaxValue || Health > short.MaxValue || Soul > short.MaxValue)
                return;

            int total = Force + Speed + Health + Soul;
            if (total < client.Character.AttributePoints)
                return;

            await client.Character.AddAttributesAsync(ClientUpdateType.Strength, Force);
            await client.Character.AddAttributesAsync(ClientUpdateType.Agility, Speed);
            await client.Character.AddAttributesAsync(ClientUpdateType.Vitality, Health);
            await client.Character.AddAttributesAsync(ClientUpdateType.Spirit, Soul);

            await client.Character.AddAttributesAsync(ClientUpdateType.Atributes, total * -1);

            //await user.SendAsync(new MsgUserAttrib(client.Identity, ClientUpdateType.Strength, client.Character.Strength));
            //await user.SendAsync(new MsgUserAttrib(client.Identity, ClientUpdateType.Agility, client.Character.Agility));
            //await user.SendAsync(new MsgUserAttrib(client.Identity, ClientUpdateType.Vitality, client.Character.Vitality));
            //await user.SendAsync(new MsgUserAttrib(client.Identity, ClientUpdateType.Spirit, client.Character.Spirit));
            //await user.SendAsync(new MsgUserAttrib(client.Identity, ClientUpdateType.Atributes, client.Character.AttributePoints));

            await user.SaveAsync();
        }
    }
}
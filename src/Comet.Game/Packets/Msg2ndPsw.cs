using Comet.Game.States;
using Comet.Network.Packets;
using Comet.Shared;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class Msg2ndPsw : MsgBase<Client>
    {
        public PasswordRequestType Action { get; set; }
        public uint Password { get; set; }
        public uint NewPassword { get; set; }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Action = (PasswordRequestType)reader.ReadInt32();
            Password = reader.ReadUInt32();
            NewPassword = reader.ReadUInt32();
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort) PacketType.Msg2ndPsw);
            writer.Write((int)Action);
            writer.Write(Password);
            writer.Write(NewPassword);
            return writer.ToArray();
        }

        public override async Task ProcessAsync(Client client)
        {
            Character user = client.Character;
            switch (Action)
            {
                case PasswordRequestType.VerifyPassword:
                    {
                        if (!user.IsUnlocked() && user.CanUnlock2ndPassword())
                        {
                            if (user.SecondaryPassword != NewPassword)
                            {
                                Action = PasswordRequestType.WrongPassword;
                                await user.SendAsync(this);
                                return;
                            }

                            user.UnlockSecondaryPassword();
                            Action = PasswordRequestType.CorrectPassword;
                            await user.SendAsync(this);
                        }
                        else if (!user.IsUnlocked() && !user.CanUnlock2ndPassword())
                        {
                            Action = PasswordRequestType.SendInformation;
                            await user.SendAsync(this);
                            return;
                        }
                        break;
                    }

                case PasswordRequestType.SetNewPassword:
                    {
                        if (user.SecondaryPassword != 0)
                        {
                            return;
                        }

                        user.SecondaryPassword = NewPassword;
                        await user.SaveAsync();

                        Action = PasswordRequestType.CorrectPassword;
                        Password = 0x0101;
                        await user.SendAsync(this);
                        break;
                    }
                default:
                    {
                        await Log.WriteLogAsync(LogLevel.Socket, $"Invalid Msg2ndPsw:Action ({Action})");
                        await Log.WriteLogAsync(LogLevel.Socket, PacketDump.Hex(Encode()));
                        break;
                    }
            }
        }

        public enum PasswordRequestType
        {
            Unknown,
            VerifyPassword,
            ForgotPassword,
            SetNewPassword,
            SendInformation,
            CorrectPassword,
            WrongPassword
        }
    }
}

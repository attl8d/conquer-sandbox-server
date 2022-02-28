using Comet.Game.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Comet.Game.States
{
    public static class MessageBoard
    {
        private static Dictionary<uint, MessageInfo> m_dicTrade = new Dictionary<uint, MessageInfo>();
        private static Dictionary<uint, MessageInfo> m_dicTTeam = new Dictionary<uint, MessageInfo>();
        private static Dictionary<uint, MessageInfo> m_dicFriend = new Dictionary<uint, MessageInfo>();
        private static Dictionary<uint, MessageInfo> m_dicSyndicate = new Dictionary<uint, MessageInfo>();
        private static Dictionary<uint, MessageInfo> m_dicOther = new Dictionary<uint, MessageInfo>();
        private static Dictionary<uint, MessageInfo> m_dicSystem = new Dictionary<uint, MessageInfo>();

        public static bool AddMessage(Character user, string message, MsgTalk.TalkChannel channel)
        {
            Dictionary<uint, MessageInfo> board;
            switch (channel)
            {
                case MsgTalk.TalkChannel.TradeBoard:
                    board = m_dicTrade;
                    break;
                case MsgTalk.TalkChannel.TeamBoard:
                    board = m_dicTTeam;
                    break;
                case MsgTalk.TalkChannel.FriendBoard:
                    board = m_dicFriend;
                    break;
                case MsgTalk.TalkChannel.GuildBoard:
                    board = m_dicSyndicate;
                    break;
                case MsgTalk.TalkChannel.OthersBoard:
                    board = m_dicOther;
                    break;
                case MsgTalk.TalkChannel.Bbs:
                    board = m_dicSystem;
                    break;
                default:
                    return false;
            }

            if (board.ContainsKey(user.Identity))
                board.Remove(user.Identity);

            // todo verify silence
            // todo filter words

            board.Add(user.Identity, new MessageInfo
            {
                SenderIdentity = user.Identity,
                Sender = user.Name,
                Message = message.Substring(0, Math.Min(message.Length, 255)),
                Time = DateTime.Now
            });

            return true;
        }

        public static List<MessageInfo> GetMessages(MsgTalk.TalkChannel channel, int page)
        {
            List<MessageInfo> msgs;
            switch (channel)
            {
                case MsgTalk.TalkChannel.TradeBoard:
                    msgs = m_dicTrade.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.TeamBoard:
                    msgs = m_dicTTeam.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.FriendBoard:
                    msgs = m_dicFriend.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.GuildBoard:
                    msgs = m_dicSyndicate.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.OthersBoard:
                    msgs = m_dicOther.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.Bbs:
                    msgs = m_dicSystem.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                default:
                    return new List<MessageInfo>();
            }

            if (page * 8 > msgs.Count)
                return new List<MessageInfo>();

            return msgs.Skip(page * 8).Take(8).ToList();
        }

        public static string GetMessage(string name, MsgTalk.TalkChannel channel)
        {
            List<MessageInfo> msgs;
            switch (channel)
            {
                case MsgTalk.TalkChannel.TradeBoard:
                    msgs = m_dicTrade.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.TeamBoard:
                    msgs = m_dicTTeam.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.FriendBoard:
                    msgs = m_dicFriend.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.GuildBoard:
                    msgs = m_dicSyndicate.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.OthersBoard:
                    msgs = m_dicOther.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                case MsgTalk.TalkChannel.Bbs:
                    msgs = m_dicSystem.Values.OrderByDescending(x => x.Time).ToList();
                    break;
                default:
                    return string.Empty;
            }

            return msgs.FirstOrDefault(x => x.Sender.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Message ?? string.Empty;
        }
    }

    public struct MessageInfo
    {
        public uint SenderIdentity;
        public string Sender;
        public string Message;
        public DateTime Time;
    }
}

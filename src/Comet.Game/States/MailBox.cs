using Comet.Core;
using Comet.Game.Database;
using Comet.Game.Database.Models;
using Comet.Game.Database.Repositories;
using Comet.Game.Packets;
using Comet.Game.States.Items;
using Comet.Shared;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.States
{
    public sealed class MailBox
    {
        public const int PageSize = 7;

        private TimeOut mCheckMails = new TimeOut(60);
        private Character mUser;
        private bool mNotified = false;
        private ConcurrentDictionary<uint, MailMessage> mDictMail = new ConcurrentDictionary<uint, MailMessage>();

        private MailBox()
        {

        }

        private async Task QueryAsync()
        {
            var mails = await DbMail.GetListAsync(mUser.Identity);
            foreach (var mail in mails)
            {
                if (!mDictMail.ContainsKey(mail.Identity))
                {
                    mDictMail.TryAdd(mail.Identity, new MailMessage(mail));
                }
            }
        }

        public static async Task<MailBox> LoadAsync(Character owner)
        {
            if (owner == null)
                return null;

            MailBox mailBox = new ()
            {
                mUser = owner
            };
            await mailBox.QueryAsync();
            await mailBox.NotifyAsync();
            return mailBox;
        }

        public static async Task<bool> SendAsync(uint target, string senderName, string header, string body, uint expiration,
            uint money = 0, uint emoney = 0, uint item = 0, uint action = 0)
        {
            DbCharacter dbTarget = await CharactersRepository.FindByIdentityAsync(target);
            if (dbTarget == null)
                return false; // target not found

            DbMail mail = new()
            {
                ReceiverId = target,
                SenderName = senderName,
                Title = header,
                Content = body,
                Action = action,
                Money = money,
                ConquerPoints = emoney,
                ExpirationDate = expiration,
                ItemIdentity = item                
            };
            if (!await BaseRepository.SaveAsync(mail))
                return false;
            return true;
        }

        public int UnreadMessages => mDictMail.Count(x => !x.Value.IsRead);

        public async Task SendListAsync(int page)
        {
            int from = page;
            MsgMailList msg = new ();
            msg.Page = page;
            msg.MaxPages = (ushort)Math.Ceiling(mDictMail.Count / (double) PageSize);
            foreach (var mail in mDictMail.Values
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Order).ThenBy(x => x.Expiration)
                .Skip(from)
                .Take(PageSize))
            {
                msg.MailList.Add(new MsgMailList.MailListStruct
                {
                    EmailIdentity = mail.Identity,
                    SenderName = mail.SenderName,
                    Header = mail.Title,
                    Money = (uint)mail.Money,
                    ConquerPoints = mail.ConquerPoints,
                    ItemType = mail.Action != 0 || mail.Item != 0 ? 1u : 0u,
                    Timestamp = (int) mail.Expiration,
                    HasAttachment = mail.Action != 0 ? 1 : 0
                });

                if (!mail.IsNotified)
                    await mail.SetFlagAsync(MailMessage.MessageFlag.Notified);
            }
            await mUser.SendAsync(msg);
        }

        public async Task SendMessageAsync(uint idMail)
        {
            if (!mDictMail.TryGetValue(idMail, out var mail) || mail.IsDeleted)
                return;

            if (!mail.IsRead)
                await mail.SetFlagAsync(MailMessage.MessageFlag.Read);

            await mUser.SendAsync(new MsgMailContent
            {
                Data = idMail,
                Content = mail.Content
            });
        }

        public async Task DeleteMessageAsync(uint idMail)
        {
            if (!mDictMail.TryGetValue(idMail, out var mail) || mail.IsDeleted)
                return;

            await mail.SetFlagAsync(MailMessage.MessageFlag.Deleted);
        }

        public async Task ClaimMoneyAsync(uint idMail)
        {
            if (!mDictMail.TryGetValue(idMail, out var mail) || mail.IsDeleted)
                return;

            if (mail.HasClaimedMoney)
                return;

            await mUser.AwardMoneyAsync((int)mail.Money);
            await mail.SetFlagAsync(MailMessage.MessageFlag.MoneyClaimed);
        }

        public async Task ClaimConquerPointsAsync(uint idMail)
        {
            if (!mDictMail.TryGetValue(idMail, out var mail) || mail.IsDeleted)
                return;

            if (mail.HasClaimedConquerPoints)
                return;

            await mUser.AwardConquerPointsAsync((int)mail.ConquerPoints);
            await mail.SetFlagAsync(MailMessage.MessageFlag.EmoneyClaimed);
        }

        public async Task ClaimActionAsync(uint idMail)
        {
            if (!mDictMail.TryGetValue(idMail, out var mail) || mail.IsDeleted)
                return;

            if (mail.HasClaimedAction)
                return;

            await GameAction.ExecuteActionAsync(mail.Action, mUser, null, null, null);
            await mail.SetFlagAsync(MailMessage.MessageFlag.ActionClaimed);
        }

        public async Task ClaimItemAsync(uint idMail)
        {
            if (!mDictMail.TryGetValue(idMail, out var mail) || mail.IsDeleted)
                return;

            if (mail.HasClaimedItem)
                return;

            DbItem dbItem = await ItemRepository.GetByIdAsync(mail.Item);
            if (dbItem?.PlayerId != 0)
                return;

            Item item = new Item(mUser);
            if (!await item.CreateAsync(dbItem))
                return;

            if (!mUser.UserPackage.IsPackSpare((int)item.AccumulateNum, item.Type))
                return;

            await mUser.UserPackage.AddItemAsync(item);
            await mail.SetFlagAsync(MailMessage.MessageFlag.ItemClaimed);
        }

        public Task NotifyAsync()
        {
            return mUser.SendAsync(new MsgMailNotify
            {
                Action = MsgMailNotify.MailNotification.Notification
            });
        }

        public async Task OnTimerAsync()
        {
            if (!mCheckMails.ToNextTime())
                return;

            await QueryAsync();

            if (UnreadMessages > 0)
                await NotifyAsync();
        }
    }

    public sealed class MailMessage
    {
        [Flags]
        public enum MessageFlag : byte
        {
            None = 0,
            Read = 0x1,
            Deleted = 0x2,
            MoneyClaimed = 0x4,
            EmoneyClaimed = 0x8,
            ItemClaimed = 0x10,
            Notified = 0x20,
            ActionClaimed = 0x40
        }

        private DbMail mDbMail;

        public MailMessage(DbMail mail)
        {
            mDbMail = mail;
        }

        private MessageFlag Flag => (MessageFlag) mDbMail.Flag;

        public bool IsRead => (Flag & MessageFlag.Read) != 0;
        public bool IsDeleted => (Flag & MessageFlag.Deleted) != 0;
        public bool HasClaimedMoney => (Flag & MessageFlag.MoneyClaimed) != 0;
        public bool HasClaimedConquerPoints => (Flag & MessageFlag.EmoneyClaimed) != 0;
        public bool HasClaimedItem => (Flag & MessageFlag.ItemClaimed) != 0;
        public bool HasClaimedAction => (Flag & MessageFlag.ActionClaimed) != 0;
        public bool IsNotified => (Flag & MessageFlag.Notified) != 0;
        public bool HasExpired => UnixTimestamp.Now() > Expiration;

        public int Order => !IsRead || !HasClaimedConquerPoints || !HasClaimedItem || !HasClaimedMoney ? 1 : 0;

        public uint Identity => mDbMail.Identity;
        public string SenderName => mDbMail.SenderName;
        public string Title => mDbMail.Title;
        public string Content => mDbMail.Content;

        public ulong Money => mDbMail.Money;
        public uint ConquerPoints => mDbMail.ConquerPoints;
        public uint Item => mDbMail.ItemIdentity;
        public uint Action => mDbMail.Action;
        public uint Expiration => mDbMail.ExpirationDate;


        public Task SetFlagAsync(MessageFlag flag)
        {
            mDbMail.Flag |= (byte) flag;
            return BaseRepository.SaveAsync(mDbMail);
        }
    }
}
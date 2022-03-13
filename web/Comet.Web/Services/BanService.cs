using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Comet.Database.Entities;
using Comet.Web.Areas.Panel.Models;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Identity;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Services
{
    public class BanService
    {
        private readonly ApplicationDbContext mContext;
        private readonly IEmailSender mEmailSender;
        private readonly LanguageService mLanguage;
        private readonly LogService<BanService> mLog;
        private readonly RoleManager mRoleManager;
        private readonly UserManager mUserManager;

        public BanService(ApplicationDbContext context,
                          UserManager userManager,
                          RoleManager roleManager,
                          LanguageService language,
                          LogService<BanService> log,
                          IEmailSender emailSender)
        {
            mContext = context;
            mUserManager = userManager;
            mRoleManager = roleManager;
            mLanguage = language;
            mLog = log;
            mEmailSender = emailSender;
        }

        public Task<ApplicationUserBan> GetUserBanByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return mContext.ApplicationUserBans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<List<ApplicationUserBanReason>> GetBanReasonsAsync(
            int type, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return mContext.ApplicationUserBanReasons.Where(x => x.Type == type).ToListAsync(cancellationToken);
        }

        public Task<ApplicationUserBanReason> GetBanReasonAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return mContext.ApplicationUserBanReasons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<AdminBanListModel>> GetAsync(int page,
                                                            int ipp = 25,
                                                            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            page = Math.Max(page - 1, 0);

            List<ApplicationUserBan> tempResult = await mContext.ApplicationUserBans
                                                                .OrderBy(x => x.Flags != 0)
                                                                .Skip(page * ipp)
                                                                .Take(ipp)
                                                                .ToListAsync(cancellationToken);

            var result = new List<AdminBanListModel>();

            foreach (ApplicationUserBan item in tempResult)
            {
                ApplicationUserBanReason reason = await mContext.ApplicationUserBanReasons.FirstOrDefaultAsync(
                                                      x => x.Id == item.ReasonId, cancellationToken);
                string userName = mLanguage.GetString("InvalidAccount");
                if (reason.Type == 0)
                {
                    ApplicationUser account = await mContext.Users
                                                            .FirstOrDefaultAsync(
                                                                x => x.Id == item.AccountId, cancellationToken);
                    if (account != null)
                        userName = account.UserName;
                }
                else
                {
                    userName = "TODO Player Name";
                }

                string bannedBy = mLanguage.GetString("FormerModerator");
                ApplicationUser moderator =
                    await mContext.Users.FirstOrDefaultAsync(x => x.Id == item.BannedBy, cancellationToken);
                if (moderator != null)
                    bannedBy = moderator.UserName;

                result.Add(new AdminBanListModel
                {
                    BanType = reason.Type == 0 ? mLanguage.GetString("Account") : mLanguage.GetString("Game"),
                    Id = item.Id,
                    UserName = userName,
                    BannedBy = bannedBy,
                    Reason = mLanguage.GetString(reason.Name),
                    From = item.BanTime,
                    To = item.ExpireTime,
                    Flags = item.Flags
                });
            }

            return result;
        }

        public async Task<int> BanAccountAsync(ApplicationUser admin, ApplicationUser target, int reasonId,
                                               string description)
        {
            ApplicationUserBanReason reason =
                await mContext.ApplicationUserBanReasons.FirstOrDefaultAsync(x => x.Id == reasonId);
            if (reason == null)
                return StatusCodes.Status500InternalServerError;

            BanReport info = await mUserManager.GetBanInformationAsync(target);
            if (info == null)
                return StatusCodes.Status404NotFound;

            BanReport.BanTerm term = await mUserManager.QueryNextBanTermAsync(target, reasonId);
            if (term == null)
                return StatusCodes.Status400BadRequest;

            if (info.IsAccountBanned)
                return StatusCodes.Status200OK;

            DateTime expireTime;
            if (term.IsPermanent)
                expireTime = DateTime.MaxValue;
            else
                expireTime = DateTime.Now.AddMinutes(term.Minutes);

            ApplicationUserBan ban = new()
            {
                AccountId = target.Id,
                BannedBy = admin.Id,
                BanTime = DateTime.Now,
                ExpireTime = expireTime,
                Data = 0,
                ReasonMessage = description,
                ReasonId = reasonId
            };

            if (!await mContext.SaveAsync(ban))
                return StatusCodes.Status500InternalServerError;

            var subject = "";
            var message = "";

            if (reason.Type == 0)
            {
                subject = mLanguage.GetString("AccountBanEmailSubject", target.UserName);
                message = mLanguage.GetString("AccountBanEmailMessage");
            }

            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(message))
                await mEmailSender.SendEmailAsync(target.Email, subject, message);

            await mLog.LogAsync($"Moderator {admin.UserName} has banned {target.UserName}",
                                LogHistory.LogType.AccountBan, LogLevel.Information, new
                                {
                                    banId = ban.Id,
                                    adminId = admin.Id,
                                    targetId = target.Id,
                                    from = ban.BanTime,
                                    to = ban.ExpireTime
                                }, admin.Id, target.Id);

            return StatusCodes.Status200OK;
        }

        public async Task<int> CancelBanAsync(ClaimsPrincipal user, int userId, int banId)
        {
            ApplicationUserBan ban = await mContext.ApplicationUserBans
                                                   .Include(x => x.Reason)
                                                   .FirstOrDefaultAsync(x => x.Id == banId);
            if (ban == null)
                return StatusCodes.Status404NotFound;

            var flag = (BanFlags) ban.Flags;
            if (flag.HasFlag(BanFlags.Removed) || flag.HasFlag(BanFlags.Forgiven) ||
                flag.HasFlag(BanFlags.FalsePositive))
                return StatusCodes.Status409Conflict;

            ApplicationUser admin = await mUserManager.GetUserAsync(user);
            if (admin == null)
                return StatusCodes.Status401Unauthorized;

            ApplicationUser target = await mUserManager.FindByIdAsync(ban.AccountId.ToString());
            if (target == null)
                return StatusCodes.Status404NotFound;

            if (ban.Reason.Type == 0) // account
            {
                if (!user.HasPermission(Security.Claims.CancelAccountBan))
                    return StatusCodes.Status401Unauthorized;
            }
            else // game account
            {
                if (!user.HasPermission(Security.Claims.CancelGameBan))
                    return StatusCodes.Status401Unauthorized;
            }

            ban.Flags |= (int) BanFlags.Removed;
            await mContext.SaveAsync(ban);

            if (ban.Reason.Type == 1)
            {
                DbAccount gameAccount = await mContext.GameAccounts.FirstOrDefaultAsync(x => x.AccountID == ban.Data);
                if (gameAccount != null && gameAccount.StatusID.HasFlag(DbAccount.AccountStatus.Banned))
                {
                    gameAccount.StatusID &= ~DbAccount.AccountStatus.Banned;
                    await mContext.SaveAsync(gameAccount);
                }
            }

            await mLog.LogAsync($"Moderator {admin.UserName} has flag user {target.UserName} ban as Cancelled",
                                LogHistory.LogType.CancelBan, LogLevel.Information, new
                                {
                                    banId = ban.Id,
                                    adminId = admin.Id,
                                    targetId = target.Id,
                                    from = ban.BanTime,
                                    to = ban.ExpireTime,
                                    data = ban.Data
                                }, admin.Id, target.Id);

            return StatusCodes.Status200OK;
        }

        public async Task<int> ForgiveBanAsync(ClaimsPrincipal user, int userId, int banId)
        {
            ApplicationUserBan ban = await mContext.ApplicationUserBans
                                                   .Include(x => x.Reason)
                                                   .FirstOrDefaultAsync(x => x.Id == banId);
            if (ban == null)
                return StatusCodes.Status404NotFound;

            var flag = (BanFlags) ban.Flags;
            if (flag.HasFlag(BanFlags.Removed) || flag.HasFlag(BanFlags.Forgiven) ||
                flag.HasFlag(BanFlags.FalsePositive))
                return StatusCodes.Status409Conflict;

            ApplicationUser admin = await mUserManager.GetUserAsync(user);
            if (admin == null)
                return StatusCodes.Status401Unauthorized;

            ApplicationUser target = await mUserManager.FindByIdAsync(ban.AccountId.ToString());
            if (target == null)
                return StatusCodes.Status404NotFound;

            if (ban.Reason.Type == 0) // account
            {
                if (!user.HasPermission(Security.Claims.ForgiveAccountBan))
                    return StatusCodes.Status401Unauthorized;
            }
            else // game account
            {
                if (!user.HasPermission(Security.Claims.ForgiveGameBan))
                    return StatusCodes.Status401Unauthorized;
            }

            ban.Flags |= (int) BanFlags.Forgiven;
            await mContext.SaveAsync(ban);

            if (ban.Reason.Type == 1)
            {
                DbAccount gameAccount = await mContext.GameAccounts.FirstOrDefaultAsync(x => x.AccountID == ban.Data);
                if (gameAccount != null && gameAccount.StatusID.HasFlag(DbAccount.AccountStatus.Banned))
                {
                    gameAccount.StatusID &= ~DbAccount.AccountStatus.Banned;
                    await mContext.SaveAsync(gameAccount);
                }
            }

            await mLog.LogAsync($"Moderator {admin.UserName} has flag user {target.UserName} ban as Forgiven",
                                LogHistory.LogType.CancelBan, LogLevel.Information, new
                                {
                                    banId = ban.Id,
                                    adminId = admin.Id,
                                    targetId = target.Id,
                                    from = ban.BanTime,
                                    to = ban.ExpireTime,
                                    data = ban.Data
                                }, admin.Id, target.Id);

            return StatusCodes.Status200OK;
        }

        public async Task<int> FalsePositiveBanAsync(ClaimsPrincipal user, int userId, int banId)
        {
            ApplicationUserBan ban = await mContext.ApplicationUserBans
                                                   .Include(x => x.Reason)
                                                   .FirstOrDefaultAsync(x => x.Id == banId);
            if (ban == null)
                return StatusCodes.Status404NotFound;

            var flag = (BanFlags) ban.Flags;
            if (flag.HasFlag(BanFlags.FalsePositive))
                return StatusCodes.Status409Conflict;

            ApplicationUser admin = await mUserManager.GetUserAsync(user);
            if (admin == null)
                return StatusCodes.Status401Unauthorized;

            ApplicationUser target = await mUserManager.FindByIdAsync(ban.AccountId.ToString());
            if (target == null)
                return StatusCodes.Status404NotFound;

            if (ban.Reason.Type == 0) // account
            {
                if (!user.HasPermission(Security.Claims.SignalFalsePositiveAccountBan))
                    return StatusCodes.Status401Unauthorized;
            }
            else // game account
            {
                if (!user.HasPermission(Security.Claims.SignalFalsePositiveGameBan))
                    return StatusCodes.Status401Unauthorized;
            }

            ban.Flags |= (int) BanFlags.FalsePositive;
            await mContext.SaveAsync(ban);

            if (ban.Reason.Type == 1)
            {
                DbAccount gameAccount = await mContext.GameAccounts.FirstOrDefaultAsync(x => x.AccountID == ban.Data);
                if (gameAccount != null && gameAccount.StatusID.HasFlag(DbAccount.AccountStatus.Banned))
                {
                    gameAccount.StatusID &= ~DbAccount.AccountStatus.Banned;
                    await mContext.SaveAsync(gameAccount);
                }
            }

            await mLog.LogAsync($"Moderator {admin.UserName} has flag user {target.UserName} ban as False Positive",
                                LogHistory.LogType.CancelBan, LogLevel.Information, new
                                {
                                    banId = ban.Id,
                                    adminId = admin.Id,
                                    targetId = target.Id,
                                    from = ban.BanTime,
                                    to = ban.ExpireTime,
                                    data = ban.Data
                                }, admin.Id, target.Id);

            return StatusCodes.Status200OK;
        }
    }
}
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Model.Account;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Comet.Web.App.Services.Security;

namespace Comet.Web.App.API
{
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = Moderator)]
    public class BansController : ControllerBase
    {
        private readonly ApplicationDbContext mDbContext;
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly SignInManager mSignInManager;
        private readonly LogService<BansController> mLog;
        private readonly IEmailSender mEmailSender;

        public BansController(ApplicationDbContext dbContext,
            UserManager userManager,
            SignInManager signInManager,
            LanguageService language, 
            LogService<BansController> log,
            IEmailSender emailService)
        {
            mDbContext = dbContext;
            mUserManager = userManager;
            mLanguage = language;
            mSignInManager = signInManager;
            mLog = log;
            mEmailSender = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(int page, int ipp = 25)
        {
            page = Math.Max(page - 1, 0);
            var tempResult = await mDbContext.ApplicationUserBans
                .OrderBy(x => x.Flags != 0)
                .Skip(page * ipp)
                .Take(ipp)
                .ToListAsync();

            List<object> result = new List<object>();
            foreach (var item in tempResult)
            {
                var reason = await mDbContext.ApplicationUserBanReasons.FirstOrDefaultAsync(x => x.Id == item.ReasonId);
                string userName = mLanguage.GetString("InvalidAccount");
                if (reason.Type == 0)
                {
                    var account = await mDbContext.Users.FirstOrDefaultAsync(x => x.Id == item.AccountId);
                    if (account != null)
                        userName = account.UserName;
                }
                else
                {
                    userName = "TODO Player Name";
                }
                string bannedBy = mLanguage.GetString("FormerModerator");
                var moderator = await mDbContext.Users.FirstOrDefaultAsync(x => x.Id == item.BannedBy);
                if (moderator != null)
                    bannedBy = moderator.UserName;

                result.Add(new 
                {
                    banType = reason.Type == 0 ? mLanguage.GetString("Account") : mLanguage.GetString("Game"),
                    item.Id,
                    userName,
                    bannedBy,
                    reason = mLanguage.GetString(reason.Name),
                    from = item.BanTime,
                    to = item.ExpireTime
                });
            }

            return Ok(result);
        }

        [HttpGet(Name = "QueryAccountBans")]
        public async Task<IActionResult> QueryAccountBansAsync(int userId)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound();

            var bans = await mUserManager.GetBanInformationAsync(user);

            return Ok(bans.Account);
        }

        [HttpPost(Name = "GetUserBanFactor")]
        public async Task<IActionResult> GetUserBanFactorAsync([FromBody] UserCheckBanModel model)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(model.userId.ToString());
            if (user == null)
                return NotFound();

            var factor = await mUserManager.QueryNextBanTermAsync(user, model.reason);
            if (factor == null)
                return BadRequest();

            return Ok(factor);
        }

        [HttpPost(Name = "DoBanAccount")]
        public async Task<IActionResult> DoBanAccountAsync([FromBody] UserBanModel model)
        {
            if (!User.HasPermission(Claims.BanAccount))
                return Unauthorized();

            ApplicationUser user = await mUserManager.FindByIdAsync(model.userId.ToString());
            if (user == null)
                return NotFound(mLanguage.GetString("UserNotFound"));

            var reason = await mDbContext.ApplicationUserBanReasons.FirstOrDefaultAsync(x => x.Id == model.reason);
            if (reason == null)
            {
                return StatusCode(500, "Invalid Reason");
            }

            var info = await mUserManager.GetBanInformationAsync(user);
            if (info == null)
                return NotFound(mLanguage.GetString("NoUserData"));

            var term = await mUserManager.QueryNextBanTermAsync(user, model.reason);
            if (term == null)
                return BadRequest(mLanguage.GetString("InvalidBanType"));

            if (info.IsAccountBanned)
                return Ok(mLanguage.GetString("UserAlreadyBanned"));

            ApplicationUser admin = await mUserManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized();

            var principal = await mSignInManager.CreateUserPrincipalAsync(user);

            if (!User.IsInRole(Roles.SuperAdministrator) && principal.IsInRole(Roles.Administrator))
                return Unauthorized();

            if (!User.IsInRole(Roles.SuperAdministrator) && principal.HasPermission(Claims.CannotBeBanned))
                return Unauthorized(mLanguage.GetString("UserCannotBeBanned"));

            ApplicationUserBan ban = new()
            {
                AccountId = user.Id,
                BannedBy = admin.Id,
                BanTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(term.Minutes),
                Data = 0,
                Reason = model.description,
                ReasonId = model.reason
            };

            mDbContext.ApplicationUserBans.Add(ban);
            try
            {
                int result = await mDbContext.SaveChangesAsync();
                if (result <= 0)
                {
                    ModelState.AddModelError("DbError", "Não foi possível gravar as informações no banco de dados.");
                    return StatusCode(500, ModelState);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            string subject = "";
            string message = "";

            if (reason.Type == 0)
            {
                subject = mLanguage.GetString("AccountBanEmailSubject", user.UserName);
                message = mLanguage.GetString("AccountBanEmailMessage");
            }
            else
            {

            }

            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(message))
            {
                await mEmailSender.SendEmailAsync(user.Email, subject, message);
            }

            await mLog.LogAsync($"Moderator {admin.UserName} has banned {user.UserName}", LogHistory.LogType.AccountBan, LogLevel.Information, new
            {
                banId = ban.Id,
                adminId = admin.Id,
                targetId = user.Id,
                from = ban.BanTime,
                to = ban.ExpireTime
            }, admin.Id, user.Id);
            return Ok();
        }

        [HttpPost(Name = "CancelBan")]
        public async Task<IActionResult> CancelBanAsync([FromBody] UserRemoveBalModel model)
        {
            ApplicationUser admin = await mUserManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized();

            var ban = await mDbContext.ApplicationUserBans.FirstOrDefaultAsync(x => x.Id == model.banId);
            if (ban == null)
                return NotFound();

            ApplicationUser user = await mUserManager.FindByIdAsync(ban.AccountId.ToString());
            if (user == null)
                return NotFound(mLanguage.GetString("UserNotFound"));

            var reason = await mDbContext.ApplicationUserBanReasons.FirstOrDefaultAsync(x => x.Id == ban.ReasonId);
            if (reason == null)
            {
                return StatusCode(500, "Invalid Reason");
            }    

            if (reason.Type == 0)
            {
                if (!User.HasPermission(Claims.CancelAccountBan))
                {
                    return Forbid();
                }
            }
            else
            {
                if (!User.HasPermission(Claims.CancelGameBan))
                {
                    return Forbid();
                }
            }

            BanFlags flag = (BanFlags)ban.Flags;
            if (flag.HasFlag(BanFlags.Removed) || flag.HasFlag(BanFlags.Forgiven) || flag.HasFlag(BanFlags.FalsePositive))
                return BadRequest("Ban already cancelled.");

            ban.Flags |= (int) BanFlags.Removed;
            mDbContext.Update(ban);
            await mDbContext.SaveChangesAsync();

            if (reason.Type == 1)
            {
                // TODO remove ban from game account
            }

            await mLog.LogAsync($"Moderator {admin.UserName} has flag user {user.UserName} ban as Cancelled", LogHistory.LogType.CancelBan, LogLevel.Information, new
            {
                banId = ban.Id,
                adminId = admin.Id,
                targetId = user.Id,
                from = ban.BanTime,
                to = ban.ExpireTime,
                data = ban.Data
            }, admin.Id, user.Id);

            return Ok();
        }

        [HttpPost(Name = "ForgiveBan")]
        public async Task<IActionResult> ForgiveBanAsync([FromBody] UserRemoveBalModel model)
        {
            ApplicationUser admin = await mUserManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized();

            var ban = await mDbContext.ApplicationUserBans.FirstOrDefaultAsync(x => x.Id == model.banId);
            if (ban == null)
                return NotFound();

            ApplicationUser user = await mUserManager.FindByIdAsync(ban.AccountId.ToString());
            if (user == null)
                return NotFound(mLanguage.GetString("UserNotFound"));

            var reason = await mDbContext.ApplicationUserBanReasons.FirstOrDefaultAsync(x => x.Id == ban.ReasonId);
            if (reason == null)
            {
                return StatusCode(500, "Invalid Reason");
            }

            if (reason.Type == 0)
            {
                if (!User.HasPermission(Claims.ForgiveAccountBan))
                {
                    return Forbid();
                }
            }
            else
            {
                if (!User.HasPermission(Claims.ForgiveGameBan))
                {
                    return Forbid();
                }
            }

            BanFlags flag = (BanFlags)ban.Flags;
            if (flag.HasFlag(BanFlags.Forgiven) || flag.HasFlag(BanFlags.FalsePositive))
                return BadRequest("Ban already cancelled.");

            ban.Flags |= (int)BanFlags.Forgiven;
            mDbContext.Update(ban);
            await mDbContext.SaveChangesAsync();

            if (reason.Type == 1)
            {
                // TODO remove ban from game account
            }

            await mLog.LogAsync($"Moderator {admin.UserName} has flag user {user.UserName} ban as Forgiven", LogHistory.LogType.ForgiveBan, LogLevel.Information, new
            {
                banId = ban.Id,
                adminId = admin.Id,
                targetId = user.Id,
                from = ban.BanTime,
                to = ban.ExpireTime,
                data = ban.Data
            }, admin.Id, user.Id);

            return Ok();
        }

        [HttpPost(Name = "FalsePositiveBan")]
        public async Task<IActionResult> FalsePositiveBanAsync([FromBody] UserRemoveBalModel model)
        {
            ApplicationUser admin = await mUserManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized();

            var ban = await mDbContext.ApplicationUserBans.FirstOrDefaultAsync(x => x.Id == model.banId);
            if (ban == null)
                return NotFound();

            ApplicationUser user = await mUserManager.FindByIdAsync(ban.AccountId.ToString());
            if (user == null)
                return NotFound(mLanguage.GetString("UserNotFound"));

            var reason = await mDbContext.ApplicationUserBanReasons.FirstOrDefaultAsync(x => x.Id == ban.ReasonId);
            if (reason == null)
            {
                return StatusCode(500, "Invalid Reason");
            }

            if (reason.Type == 0)
            {
                if (!User.HasPermission(Claims.SignalFalsePositiveAccountBan))
                {
                    return Forbid();
                }
            }
            else
            {
                if (!User.HasPermission(Claims.SignalFalsePositiveGameBan))
                {
                    return Forbid();
                }
            }

            BanFlags flag = (BanFlags)ban.Flags;
            if (flag.HasFlag(BanFlags.FalsePositive))
                return BadRequest("Ban already cancelled.");

            ban.Flags |= (int)BanFlags.FalsePositive;
            mDbContext.Update(ban);
            await mDbContext.SaveChangesAsync();

            if (reason.Type == 1)
            {
                // TODO remove ban from game account
            }

            await mLog.LogAsync($"Moderator {admin.UserName} has flag user {user.UserName} ban as False Positive", LogHistory.LogType.FalsePositiveBan, LogLevel.Information, new
            {
                banId = ban.Id,
                adminId = admin.Id,
                targetId = user.Id,
                from = ban.BanTime,
                to = ban.ExpireTime,
                data = ban.Data
            }, admin.Id, user.Id);
            return Ok();
        }

        public class UserRemoveBalModel
        {
            public int userId { get; set; }
            public int banId { get; set; }
        }

        public class UserBanModel
        {
            public int userId { get; set; }
            public int reason { get; set; }
            public string description { get; set; }
        }

        public class UserCheckBanModel
        {
            public int userId { get; set; }
            public int reason { get; set; }
        }
    }
}

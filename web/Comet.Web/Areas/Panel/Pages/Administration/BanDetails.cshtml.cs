using Comet.Web.Data.Entities;
using Comet.Web.Model.Identity;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using static Comet.Web.Services.Security;

namespace Comet.Web.Areas.Panel.Pages.Administration
{
    [Authorize(Policy = Moderator)]
    public class BanDetailsModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly SignInManager mSigninManager;
        private readonly LanguageService mLanguage;
        private readonly IEmailSender mEmailSender;
        private readonly BanService mBanService;

        public BanDetailsModel(UserManager userManager,
            SignInManager signInManager,
            LanguageService languageService,
            BanService banService,
            IEmailSender emailService)
        {
            mUserManager = userManager;
            mSigninManager = signInManager;
            mLanguage = languageService;
            mEmailSender = emailService;
            mBanService = banService;
        }

        [BindProperty(SupportsGet = true)]
        public int BanId { get; set; }

        public BanData Data { get; private set; }

        public bool IsGame { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUserBan ban = await mBanService.GetUserBanByIdAsync(BanId);
            if (ban == null)
                return RedirectToPage("/Bans");

            ApplicationUserBanReason reason = await mBanService.GetBanReasonAsync(ban.ReasonId);

            Data = new BanData();
            BanReport.BanTerm userBanData = null;
            if (reason.Type == 0)
            {
                ApplicationUser target = await mUserManager.FindByIdAsync(ban.AccountId.ToString());

                if (target != null)
                {
                    Data.TargetName = target.UserName;
                    userBanData = await mUserManager.QueryNextBanTermAsync(target, ban.ReasonId);
                }
            }
            else
            {
                IsGame = true;
                Data.TargetName = mLanguage.GetString("TODO Game Account");
            }

            if (userBanData != null)
            {
                Data.BanTermMessage = userBanData.Terms;
            }

            ApplicationUser bannedBy = await mUserManager.FindByIdAsync(ban.BannedBy.ToString());
            Data.BannedBy = bannedBy?.UserName ?? mLanguage.GetString("FormerModerator");

            Data.AccountId = ban.AccountId;
            Data.Reason = mLanguage.GetString(reason.Name);
            Data.Description = ban.ReasonMessage;
            Data.From = ban.BanTime;
            Data.To = ban.ExpireTime;
            Data.Id = ban.Id;
            Data.Flags = (BanFlags)ban.Flags;
            Data.IsBanned = false;

            if (Data.Flags.HasFlag(BanFlags.FalsePositive))
            {
                Data.FlagMessage = $"<span class=\"tx-warning\">{mLanguage.GetString("FalsePositive")}</span>";
            }
            else if (Data.Flags.HasFlag(BanFlags.Forgiven))
            {
                Data.FlagMessage = $"<span class=\"tx-primary\">{mLanguage.GetString("Forgiven")}</span>";
            }
            else if (Data.Flags.HasFlag(BanFlags.Removed))
            {
                Data.FlagMessage = $"<span class=\"tx-secondary\">{mLanguage.GetString("Removed")}</span>";
            }
            
            else
            {
                if (ban.ExpireTime > DateTime.Now)
                {
                    Data.IsBanned = true;
                    Data.FlagMessage = $"<span class=\"tx-danger\">{mLanguage.GetString("Banned")}</span>";
                }
                else
                {
                    if (reason.Type == 1)
                    {
                        Data.IsExpired = true;
                    }
                    Data.FlagMessage = $"<span class=\"tx-success\">{mLanguage.GetString("Expired")}</span>";
                }
            }

            return Page();
        }

        public class BanData
        {
            public int Id { get; set; }
            public int AccountId { get; set; }
            public string BanTermMessage { get; set; }
            public string Reason { get; set; }
            public string Description { get; set; }
            public string TargetName { get; set; }
            public string BannedBy { get; set; }
            public bool IsBanned { get; set; }
            public bool IsExpired { get; set; }
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public BanFlags Flags { get; set; }
            public string FlagMessage { get; set; }
        }
    }
}

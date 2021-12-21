using Comet.Web.App.Data;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Comet.Web.App.Services.Realms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Comet.Web.App.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CharactersController : ControllerBase
    {
        private readonly ApplicationDbContext mDbContext;
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly SignInManager mSignInManager;
        private readonly LogService<BansController> mLog;
        private readonly IEmailSender mEmailSender;
        private readonly RealmService mRealmService;

        public CharactersController(ApplicationDbContext dbContext,
            UserManager userManager,
            SignInManager signInManager,
            LanguageService language,
            LogService<BansController> log,
            IEmailSender emailService,
            RealmService realmService)
        {
            mDbContext = dbContext;
            mUserManager = userManager;
            mLanguage = language;
            mSignInManager = signInManager;
            mLog = log;
            mEmailSender = emailService;
            mRealmService = realmService;
        }

        [HttpGet(Name = "QueryDeletedCharacters")]
        public async Task<IActionResult> QueryDeletedCharactersAsync(string realmName)
        {
            await mRealmService.LoadRealmsAsync();

            List<DeletedCharacterInfo> result = new List<DeletedCharacterInfo>();

            ApplicationDbContext dbContext = new ApplicationDbContext();
            dbContext.ConnectionString = mRealmService.GetRealmConnectionString(realmName);

            var account = await mUserManager.GetUserAsync(User);
            var gameAccounts = await mUserManager.GetGameAccountsAsync(account);

            foreach (var gameAccount in gameAccounts)
            {
                DataTable deletedCharacters = await dbContext.SelectAsync($"CALL QueryDeletedAccounts({gameAccount.Id});");
                foreach (DataRow row in deletedCharacters.Rows)
                {
                    result.Add(new DeletedCharacterInfo
                    {
                        Id = (int)row.Field<uint>("id"),
                        Name = row.Field<string>("name"),
                        Level = row.Field<byte>("level"),
                        VipLevel = 0,
                        Account = gameAccount.Name,
                        CanBeRestored = row.Field<long>("current_character") == 0
                    });
                }
            }

            return Ok(result);
        }

        public struct DeletedCharacterInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public int VipLevel { get; set; }
            public string Account { get; set; }
            public bool CanBeRestored { get; set; }
        }
    }
}

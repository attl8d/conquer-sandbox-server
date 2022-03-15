using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Comet.Database.Entities;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Game;
using Comet.Web.Services.Identity;
using Comet.Web.Services.Realms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Encoders;

namespace Comet.Web.Services
{
    public sealed class ConquerGameService
    {
        private readonly ClaimsPrincipal mPrincipal;
        private readonly ApplicationDbContext mContext;
        private readonly LogService<ConquerGameService> mLogger;
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly IEmailSender mEmailSender;
        private readonly RealmService mRealmService;

        public ConquerGameService(ClaimsPrincipal principal,
                                  ApplicationDbContext context,
                                  LogService<ConquerGameService> logger,
                                  UserManager userManager,
                                  LanguageService language,
                                  IEmailSender emailSender,
                                  RealmService realmService)
        {
            mPrincipal = principal;
            mContext = context;
            mLogger = logger;
            mUserManager = userManager;
            mLanguage = language;
            mEmailSender = emailSender;
            mRealmService = realmService;
        }

        public async Task<int> CreateAccountAsync(string userName, string password)
        {
            ApplicationUser currentUser = await mUserManager.GetUserAsync(mPrincipal);
            DbAccount duplicate = await mContext.GameAccounts.FirstOrDefaultAsync(x => x.Username.Equals(userName));
            if (duplicate != null) return 1;

            string salt = GenerateSalt();
            var account = new DbAccount
            {
                Username = userName,
                Password = HashPassword(password, salt),
                IPAddress = "",
                AuthorityID = 1,
                ParentId = currentUser.Id,
                Registered = DateTime.Now,
                Salt = salt,
                MacAddress = ""
            };

            if (!await mContext.SaveAsync(account))
                return 2;

            await mLogger.LogAsync($"User [{currentUser.UserName}] has created game account [{account.Username}]",
                                   LogHistory.LogType.GameAccountCreation);
            return 0;
        }

        public async Task<bool> ChangePasswordAsync(int accountId, string newPassword)
        {
            ApplicationUser user = await mUserManager.GetUserAsync(mPrincipal);

            if (user == null)
                return false;

            DbAccount account = await mContext.GameAccounts.FirstOrDefaultAsync(x => x.AccountID == accountId);

            if (account == null)
                return false;

            if (user.Id != account.ParentId)
                return false;

            account.Salt = GenerateSalt();
            account.Password = HashPassword(newPassword, account.Salt);
            await mContext.SaveAsync(account);

            await mEmailSender.SendEmailAsync(user.Email,
                                              mLanguage.GetString("ChangeGamePasswordEmailSubject"),
                                              mLanguage.GetString("ChangeGamePasswordEmailHtml", account.Username,
                                                                  DateTime.Now.ToString("U")));

            await mLogger.LogAsync($"User {user.UserName} has changed {account.Username}`s password",
                                   LogHistory.LogType.ChangeGamePassword);
            return true;
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] inputHashed;
            using (var sha256 = SHA256.Create())
            {
                inputHashed = sha256.ComputeHash(Encoding.ASCII.GetBytes(password + salt));
            }

            string final = Hex.ToHexString(inputHashed);
            return final;
        }

        public static string GenerateSalt()
        {
            const string UPPER_S = "QWERTYUIOPASDFGHJKLZXCVBNM";
            const string LOWER_S = "qwertyuioplkjhgfdsazxcvbnm";
            const string NUMBER_S = "1236547890";
            const string POOL_S = UPPER_S + LOWER_S + NUMBER_S;
            const int SIZE_I = 30;

            var output = "";
            for (var i = 0; i < SIZE_I; i++)
                output += POOL_S[RandomNumberGenerator.GetInt32(int.MaxValue) % POOL_S.Length];

            return output;
        }

        public async Task<List<DeletedCharacterInfo>> QueryDeletedCharactersAsync(string serverName)
        {
            await mRealmService.LoadRealmsAsync();

            List<DeletedCharacterInfo> result = new List<DeletedCharacterInfo>();

            ApplicationDbContext dbContext = new ApplicationDbContext();
            dbContext.ConnectionString = mRealmService.GetRealmConnectionString(serverName);

            ApplicationUser account = await mUserManager.GetUserAsync(mPrincipal);
            List<GameAccountModel> gameAccounts = await mUserManager.GetGameAccountsAsync(account);

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

            return result;
        }
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
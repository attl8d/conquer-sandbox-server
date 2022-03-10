using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Game;
using Comet.Web.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Comet.Web.Services.Identity
{
    public class UserManager : UserManager<ApplicationUser>
    {
        private readonly ApplicationDbContext mContext;
        private readonly IServiceProvider mServices;
        private readonly LanguageService mLanguage;

        public UserManager(IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            IServiceProvider services,
            ApplicationDbContext dbContext,
            ILogger<UserManager<ApplicationUser>> logger,
            LanguageService language)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            mContext = dbContext;
            mServices = services;
            mLanguage = language;
        }

        public override async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var result = await base.GetClaimsAsync(user);

            Dictionary<string, Claim> dictClaims = result.ToDictionary(x => x.Type);
            // get role
            var roleUser = await mContext.UserRoles.AsQueryable().FirstOrDefaultAsync(x => x.UserId == user.Id);
            int idRole = roleUser?.RoleId ?? -1;

            // get the claims
            var roleClaims = await mContext.RoleClaims.AsQueryable().Where(x => x.RoleId == idRole).ToListAsync();
            foreach (var roleClaim in roleClaims)
            {
                if (!dictClaims.ContainsKey(roleClaim.ClaimType))
                {
                    dictClaims.Add(roleClaim.ClaimType, new Claim(roleClaim.ClaimType, roleClaim.ClaimValue));
                }
                else
                {
                    dictClaims[roleClaim.ClaimType] = new Claim(roleClaim.ClaimType, roleClaim.ClaimValue);
                }
            }

            // then we get the user claims
            var userClaims = await mContext.UserClaims.AsQueryable().Where(x => x.UserId == user.Id).ToListAsync();
            foreach (var userClaim in userClaims)
            {
                if (!dictClaims.ContainsKey(userClaim.ClaimType))
                {
                    dictClaims.Add(userClaim.ClaimType, new Claim(userClaim.ClaimType, userClaim.ClaimValue));
                }
                else
                {
                    dictClaims[userClaim.ClaimType] = new Claim(userClaim.ClaimType, userClaim.ClaimValue);
                }
            }

            return dictClaims.Select(x => x.Value).ToList();
        }

        public async Task<ProfileModel> GetUserProfileAsync(ApplicationUser user, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            string title = mLanguage.GetString(Security.Member.ToString());

            var roleUser = await mContext.UserRoles.AsQueryable().FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken: cancellationToken);
            if (roleUser != null)
            {
                var role = await mContext.Roles.AsQueryable().FirstOrDefaultAsync(x => x.Id == roleUser.RoleId, cancellationToken: cancellationToken);
                if (role != null)
                    title = mLanguage.GetString(role.Name);
            }

            var profile = new ProfileModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Title = title,
                EmailConfirmed = user.EmailConfirmed,
                PhoneConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
            };

            profile.Name = principal.FindFirst(ClaimTypes.Name)?.Value ?? user.UserName;            
            if (principal.HasClaim(x => x.Type.Equals(CustomClaimTypes.SocialName)))
            {
                profile.Name += $" ({principal.FindFirstValue(CustomClaimTypes.SocialName)})";
            }
            profile.AboutMe = principal.FindFirst(CustomClaimTypes.SelfDescription)?.Value ?? string.Empty;

            profile.Gender = principal.FindFirstValue(ClaimTypes.Gender) ?? string.Empty;
            profile.SocialName = principal.FindFirstValue(CustomClaimTypes.SocialName) ?? string.Empty;
            profile.Pronoun = principal.FindFirstValue(CustomClaimTypes.Pronoum) ?? string.Empty;

            if (DateTime.TryParse(principal.FindFirstValue(ClaimTypes.DateOfBirth), out var dob))
                profile.DateOfBirth = dob;

            profile.Locality = principal.FindFirstValue(ClaimTypes.Locality) ?? string.Empty;
            return profile;
        }

        public async Task<BanReport> GetBanInformationAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            BanReport banReport = new BanReport();
            var userBans = await mContext.ApplicationUserBans.AsQueryable().Where(x => x.AccountId == user.Id).ToListAsync(cancellationToken);
            foreach (var dbBan in userBans)
            {
                BanReport.BanInformation info = new BanReport.BanInformation();
                var banReason = await mContext.ApplicationUserBanReasons.AsQueryable().FirstOrDefaultAsync(x => x.Id == dbBan.ReasonId, cancellationToken: cancellationToken);
                if (banReason == null)
                    continue;

                if (banReason.Type == 0)
                {
                    info.Id = user.Id;
                    info.Name = user.UserName;

                    banReport.Account.Add(info);
                }
                else
                {
                    info.Id = 0;
                    info.Name = "TODO GameName";

                    banReport.Game.Add(info);
                }

                info.BanIdentity = dbBan.Id;
                info.ReasonId = banReason.Id;
                info.Reason = mLanguage.GetString(banReason.Name);
                info.Description = dbBan.Reason;
                info.From = dbBan.BanTime;
                info.To = dbBan.ExpireTime;
                info.Flags = (BanFlags)dbBan.Flags;

                var bannedBy = await mContext.Users.AsQueryable().FirstOrDefaultAsync(x => x.Id == dbBan.BannedBy, cancellationToken: cancellationToken);
                if (bannedBy == null)
                {
                    info.BannedBy = mLanguage.GetString("FormerModerator");
                }
                else
                {
                    info.BannedBy = bannedBy.UserName;
                }
            }
            return banReport;
        }

        public async Task<BanReport.BanTerm> QueryNextBanTermAsync(ApplicationUser user, int type, int data = 0, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var report = await GetBanInformationAsync(user, cancellationToken);

            var banType = await mContext.ApplicationUserBanReasons.FirstOrDefaultAsync(x => x.Id == type, cancellationToken: cancellationToken);

            if (banType == null)
                return null;

            if (banType.Type == 1 && data == 0)
                return null;

            ApplicationUserBanReasonFactor factor;
            BanReport.BanTerm term = new();

            term.Type = mLanguage.GetString(banType.Name);
            if (banType.Type == 0)
            {
                int banCount = report.Account.Count(x => x.ReasonId == type && (x.Flags & BanFlags.Forgiven) == 0);
                var factors = await mContext.ApplicationUserBanReasonsFactors.Where(x => x.ReasonId == banType.Id).ToListAsync();
                if (banCount > 0 && factors.All(x => x.Ocurrences != banCount))
                {
                    if (banCount >= factors.Max(x => x.Ocurrences))
                        factor = factors.FirstOrDefault(x => x.Ocurrences == factors.Max(y => y.Ocurrences));
                    else
                        factor = factors.FirstOrDefault();
                }
                else
                {
                    factor = factors.FirstOrDefault(x => x.Ocurrences == banCount);
                }
            }
            else
            {
                int banCount = report.Game.Count(x => x.ReasonId == type && (x.Flags & BanFlags.Forgiven) == 0);
                var factors = await mContext.ApplicationUserBanReasonsFactors.Where(x => x.ReasonId == banType.Id).ToListAsync();
                if (banCount > 0 && factors.All(x => x.Ocurrences != banCount))
                {
                    if (banCount >= factors.Max(x => x.Ocurrences))
                        factor = factors.FirstOrDefault(x => x.Ocurrences == factors.Max(y => y.Ocurrences));
                    else
                        factor = factors.FirstOrDefault();
                }
                else
                {
                    factor = factors.FirstOrDefault(x => x.Ocurrences == banCount);
                }
            }

            var termTime = TimeSpan.FromMinutes(factor.Minutes);
            string tempTimeReturn = mLanguage.GetString("BanTermsTimeDisplay", termTime.TotalDays, termTime.Hours, termTime.Minutes);

            term.IsPermanent = factor.Permanent;
            term.Minutes = factor.Minutes;
            term.Terms = factor.Permanent
                ? mLanguage.GetString("BanTermsAdmDisplayPermanent",
                    factor.AllowCancel ? mLanguage.GetString("Yes") : mLanguage.GetString("No"),
                    factor.CanBeForgiven ? mLanguage.GetString("Yes") : mLanguage.GetString("No"),
                    term.Type)
                : mLanguage.GetString("BanTermsAdmDisplay",
                    factor.AllowCancel ? mLanguage.GetString("Yes") : mLanguage.GetString("No"),
                    factor.CanBeForgiven ? mLanguage.GetString("Yes") : mLanguage.GetString("No"),
                    tempTimeReturn, term.Type);
            return term;
        }

        public async Task<List<GameAccountModel>> GetGameAccountsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var accounts = await mContext.GameAccounts
                .Where(x => x.ParentId == user.Id)
                .ToListAsync(cancellationToken);
            var bans = await GetBanInformationAsync(user, cancellationToken);
            List<GameAccountModel> result = new();

            foreach (var account in accounts)
            {
                result.Add(new GameAccountModel
                {
                    Id = (int)account.AccountID,
                    Name = account.Username,
                    BanExpire = bans.Game.FirstOrDefault(x => x.Id == account.AccountID && x.IsActive)?.To
                    // TODO add game characters to list
                });
            }

            return result;
        }

        public Task<List<ApplicationUser>> GetUsersAsync(int page, int ipp, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            return mContext.Users.Skip(Math.Max(0, (page - 1)) * ipp)
                .Take(ipp)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ApplicationUser>> GetUsersByRoleAsync(int roleId, int page = 0, int ipp = 10, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            var userIds = await mContext.UserRoles.Where(x => x.RoleId == roleId).Select(x => x.UserId).ToArrayAsync(cancellationToken);
            return await mContext.Users.Where(x => userIds.Any(y => y == x.Id)).ToListAsync(cancellationToken);
        }
    }
}

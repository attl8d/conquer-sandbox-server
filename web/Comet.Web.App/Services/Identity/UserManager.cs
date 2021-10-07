using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
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

namespace Comet.Web.App.Services.Identity
{
    public class UserManager : UserManager<ApplicationUser>
    {
        private readonly ApplicationDbContext mDbContext;
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
            mDbContext = dbContext;
            mServices = services;
            mLanguage = language;
        }

        public override async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var result = await base.GetClaimsAsync(user);

            Dictionary<string, Claim> dictClaims = result.ToDictionary(x => x.Type);

            //// get role
            var roleUser = await mDbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            int idRole = roleUser?.RoleId ?? -1;

            // get the claims
            var roleClaims = await mDbContext.RoleClaims.Where(x => x.RoleId == idRole).ToListAsync();
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
            var userClaims = await mDbContext.UserClaims.Where(x => x.UserId == user.Id).ToListAsync();
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

        public async Task<ProfileModel> GetUserProfileAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            string title = mLanguage.GetString(Security.Roles.Member.ToString());

            var roleUser = await mDbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken: cancellationToken);
            if (roleUser != null)
            {
                var role = await mDbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleUser.RoleId, cancellationToken: cancellationToken);
                if (role != null)
                    title = mLanguage.GetString(role.Name);
            }

            var profile = new ProfileModel
            {
                Id = user.Id,
                Name = user.Name,
                UserName = user.UserName,
                Email = user.Email,
                Title = title,
                EmailConfirmed = user.EmailConfirmed,
                PhoneConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
            };

            var userClaims = await mDbContext.UserClaims.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken: cancellationToken);
            foreach (var contact in Enum.GetValues<ContactInfoType>())
            {
                var claim = userClaims.FirstOrDefault(x => x.ClaimType.Equals(contact.ToClaimString()));
                if (claim != null)
                    profile.ContactInfo.Add(contact, claim.ClaimValue);
            }

            foreach (var personal in Enum.GetValues<PersonalInfoType>())
            {
                var claim = userClaims.FirstOrDefault(x => x.ClaimType.Equals(personal.ToClaimString()));
                if (claim != null)
                    profile.PersonalInfo.Add(personal, claim.ClaimValue);
            }

            if (profile.PersonalInfo.ContainsKey(PersonalInfoType.SocialName))
            {
                profile.Name += $" ({profile.PersonalInfo[PersonalInfoType.SocialName]})";
            }
            return profile;
        }
    }
}

using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Comet.Web.App.Middlewares
{
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate mNext;

        public LocalizationMiddleware(RequestDelegate next)
        {
            mNext = next;
        }

        public async Task InvokeAsync(HttpContext context, LanguageService language,
            IHttpContextAccessor httpContextAccessor,
            UserManager userManager,
            SignInManager signInManager)
        {
            var User = httpContextAccessor.HttpContext.User;
            bool changeLocale = false;
            string locale = "";
            if (httpContextAccessor.HttpContext.Request.Query.ContainsKey("locale"))
            {
                string current = httpContextAccessor.HttpContext.Request.Query["locale"];
                if (language.IsLanguageAvailable(current))
                {
                    locale = current;
                    changeLocale = true;
                }
            }

            if (changeLocale)
            {
                if (User.Identity.IsAuthenticated)
                {
                    ApplicationUser user = await userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        Claim temp = User.FindFirst(PersonalInfoType.Language);
                        Claim now = new(PersonalInfoType.Language.ToClaimString(), locale);
                        if (temp != null)
                        {
                            await userManager.ReplaceClaimAsync(user, temp, now);
                        }
                        else
                        {
                            await userManager.AddClaimAsync(user, now);
                        }
                        await signInManager.RefreshSignInAsync(user);
                    }
                }
                else
                {
                    httpContextAccessor.HttpContext.Response.Cookies.Append("Locale", locale, new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.MaxValue,
                        Secure = true,
                        IsEssential = true,
                        MaxAge = TimeSpan.FromHours(8760),
                        SameSite = SameSiteMode.None
                    });
                }

                httpContextAccessor.HttpContext.Response.Redirect(httpContextAccessor.HttpContext.Request.Path);
            }

            await mNext(context);
        }
    }

    public static class LocalizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCometLocalization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LocalizationMiddleware>();
        }
    }

}

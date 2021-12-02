using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Comet.Web.App.Middlewares
{
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate mNext;
        private readonly LanguageService mLanguage;

        public LocalizationMiddleware(RequestDelegate next, 
            LanguageService language)
        {
            mNext = next;
            mLanguage = language;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //string locale = null;
            //if (!context.User.Identity.IsAuthenticated)
            //{
            //    locale = context.User.FindFirst(PersonalInfoType.Language.ToClaimString())?.Value;

            //    string acceptLanguage = context.Request.Headers["Accept-Language"].ToString().Split(";")[0].Split(",")[0];
            //    if (mLanguage.IsLanguageAvailable(acceptLanguage))
            //        locale = acceptLanguage;
            //    else
            //        locale = mLanguage.CurrentCulture.LanguageCultureName;


            //}

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

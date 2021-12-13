using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Comet.Web.App.Middlewares
{
    public class BanMiddleware
    {
        private readonly RequestDelegate mNext;

        public BanMiddleware(RequestDelegate next)
        {
            mNext = next;
        }

        public async Task InvokeAsync(HttpContext context,
            UserManager userManager)
        {
            bool isBanned = false;
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                var banState = await userManager.GetBanInformationAsync(user);
                if (banState.IsAccountBanned)
                    isBanned = true;
            }

            context.Items.Add("Banned", isBanned);
            await mNext(context);
        }
    }

    public static class BanMiddlewareExtensions
    {
        public static IApplicationBuilder UseBanMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BanMiddleware>();
        }
    }
}

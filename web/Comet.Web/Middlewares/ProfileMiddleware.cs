using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Comet.Web.Middlewares
{
    public class ProfileMiddleware
    {
        private readonly RequestDelegate mNext;

        public ProfileMiddleware(RequestDelegate next)
        {
            mNext = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager userManager, SignInManager signInManager)
        {
            string path = context.Request.Path;
            string[] splitPath = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            ClaimsPrincipal principal = null;
            ApplicationUser currDbUser = null;
            if (splitPath.Length > 1 && !splitPath[1].Equals("Settings", StringComparison.InvariantCultureIgnoreCase))
            {
                currDbUser = await userManager.FindByUserNameAsync(splitPath[1]);
                if (currDbUser != null)
                    principal = await signInManager.CreateUserPrincipalAsync(currDbUser);
            }
            else if (context.User.Identity?.IsAuthenticated == true)
            {
                currDbUser = await userManager.GetUserAsync(context.User);
                principal = context.User;
            }

            if (currDbUser != null)
            {
                ProfileModel profile = await userManager.GetUserProfileAsync(currDbUser, principal);

                if (context.User.Identity?.IsAuthenticated == true)
                {
                    ApplicationUser currentlySignedIn = await userManager.GetUserAsync(context.User);
                    if (currentlySignedIn != null && currentlySignedIn.Id == currDbUser.Id)
                    {
                        profile.LoadedByOwner = true;
                    }
                }

                if (context.Items.ContainsKey("Profile"))
                    context.Items["Profile"] = profile;
                else context.Items.Add("Profile", profile);
            }

            await mNext(context);
        }
    }

    public static class ProfileMiddlewareExtensions
    {
        public static IApplicationBuilder UseProfileMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ProfileMiddleware>();
        }
    }
}

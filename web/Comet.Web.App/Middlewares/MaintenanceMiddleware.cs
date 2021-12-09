using Comet.Web.App.Data;
using Comet.Web.App.Model.Helpers;
using Comet.Web.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Comet.Web.App.Middlewares
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate mNext;

        public MaintenanceMiddleware(RequestDelegate next)
        {
            mNext = next;
        }

        public async Task InvokeAsync(HttpContext context,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext dbContext,
            IAuthorizationService authorizationService)
        {
            bool isUnderMaintenance = await httpContextAccessor.HttpContext.IsUnderMaintenanceAsync(dbContext);
            bool userHasAccess = true;
            if (isUnderMaintenance && !httpContextAccessor.HttpContext.Request.Path.Equals("/Maintenance"))
            {
                bool notAuthorized = true;
                if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                {
                    var authorization = await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext.User, Security.Roles.Administrator);
                    if (authorization.Succeeded)
                    {
                        notAuthorized = false;
                        userHasAccess = true;
                    }
                }

                if (notAuthorized)
                {
                    if (!httpContextAccessor.HttpContext.Request.Path.Equals("/Identity/Account/Login"))
                    {
                        httpContextAccessor.HttpContext.Response.Redirect("/Maintenance");
                    }
                    userHasAccess = false;
                }
            }

            httpContextAccessor.HttpContext.Items.Add("Maintenance", isUnderMaintenance);
            httpContextAccessor.HttpContext.Items.Add("MaintenanceAccess", userHasAccess);

            await mNext(context);
        }
    }

    public static class MaintenanceMiddlewareExtensions
    {
        public static IApplicationBuilder UseMaintenanceMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MaintenanceMiddleware>();
        }
    }
}

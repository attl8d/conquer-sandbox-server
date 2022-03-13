using System.Threading.Tasks;
using Comet.Web.Data;
using Comet.Web.Model.Helpers;
using Comet.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Comet.Web.Middlewares
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
            var userHasAccess = true;
            if (isUnderMaintenance && !httpContextAccessor.HttpContext.Request.Path.Equals("/Maintenance"))
            {
                var notAuthorized = true;
                if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                {
                    AuthorizationResult authorization =
                        await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext.User,
                                                                  Security.Administrator);
                    if (authorization.Succeeded)
                    {
                        notAuthorized = false;
                        userHasAccess = true;
                    }
                }

                if (notAuthorized)
                {
                    if (!httpContextAccessor.HttpContext.Request.Path.Equals("/Login"))
                        httpContextAccessor.HttpContext.Response.Redirect("/Maintenance");
                    userHasAccess = false;
                }
            }

            if (!httpContextAccessor.HttpContext.Items.ContainsKey("Maintenance"))
                httpContextAccessor.HttpContext.Items.Add("Maintenance", isUnderMaintenance);
            if (!httpContextAccessor.HttpContext.Items.ContainsKey("MaintenanceAccess"))
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
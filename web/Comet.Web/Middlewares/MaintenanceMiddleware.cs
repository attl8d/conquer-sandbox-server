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
                                      IAuthorizationService authorizationService, 
                                      MaintenanceService service)
        {
            bool isUnderMaintenance = await service.IsUnderMaintenanceAsync();
            var userHasAccess = true;
            if (isUnderMaintenance && httpContextAccessor.HttpContext?.Request.Path.Equals("/Maintenance") != true)
            {
                var notAuthorized = true;
                if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true)
                {
                    AuthorizationResult authorization = await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext.User,Security.Administrator);
                    if (authorization.Succeeded)
                    {
                        notAuthorized = false;
                    }
                }

                if (notAuthorized)
                {
                    if (httpContextAccessor.HttpContext?.Request.Path.Equals("/Login") != true)
                        httpContextAccessor.HttpContext?.Response.Redirect("/Maintenance");
                    userHasAccess = false;
                }
            }

            if (httpContextAccessor.HttpContext?.Items.ContainsKey("Maintenance") == false)
                httpContextAccessor.HttpContext.Items.Add("Maintenance", isUnderMaintenance);
            if (httpContextAccessor.HttpContext?.Items.ContainsKey("MaintenanceAccess") == false)
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
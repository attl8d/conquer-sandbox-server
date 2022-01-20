using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Comet.Web.App.Middlewares
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate mNext;

        public SessionMiddleware(RequestDelegate next)
        {
            mNext = next;
        }

        public async Task InvokeAsync(HttpContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            if (!httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("ReadGuid", out var idSession))
            {
                httpContextAccessor.HttpContext.Response.Cookies.Append("ReadGuid", Guid.NewGuid().ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddHours(12),
                    Secure = true,
                    IsEssential = true,
                    MaxAge = TimeSpan.FromHours(12),
                    SameSite = SameSiteMode.Strict
                });
            }

            await mNext(context);
        }
    }

    public static class SessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCometSession(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionMiddleware>();
        }
    }
}

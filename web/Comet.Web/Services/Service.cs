using System.Security.Claims;
using Comet.Web.Data.Entities;
using Comet.Web.Services.Password;
using Comet.Web.Services.Realms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Comet.Web.Services
{
    public static class Service
    {
        public static IServiceCollection Register(IServiceCollection service)
        {
            service.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            service.AddTransient(s =>
            {
                IHttpContextAccessor contextAccessor = s.GetService<IHttpContextAccessor>();
                ClaimsPrincipal user = contextAccessor?.HttpContext?.User;
                return user;
            });

            service.AddTransient<ArticleService>();
            service.AddTransient<IEmailSender, EmailService>();
            service.AddTransient<GameServerDataService>();
            service.AddTransient<LanguageService>();
            service.AddTransient(typeof(LogService<>));
            service.AddTransient<IPasswordHasher<ApplicationUser>, WhirlpoolPasswordService<ApplicationUser>>();
            service.AddTransient<RealmService>();
            service.AddTransient<MaintenanceService>();
            service.AddTransient<BanService>();
            service.AddTransient<RankingService>();
            service.AddTransient<ConquerGameService>();
            service.AddTransient<DownloadsService>();
            return service;
        }
    }
}
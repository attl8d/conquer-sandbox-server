using Comet.Web.Data.Entities;
using Comet.Web.Services.Password;
using Comet.Web.Services.Realms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Comet.Web.Services
{
    public static class Service
    {
        public static IServiceCollection Register(IServiceCollection service)
        {
            service.AddTransient<ArticleService>();
            service.AddTransient<IEmailSender, EmailService>();
            service.AddTransient<GameServerDataManager>();
            service.AddTransient<LanguageService>();
            service.AddTransient(typeof(LogService<>));
            service.AddTransient<IPasswordHasher<ApplicationUser>, WhirlpoolPasswordService<ApplicationUser>>();
            service.AddTransient<RealmService>();
            service.AddTransient<MaintenanceService>();
            service.AddTransient<BanService>();
            return service;
        }
    }
}

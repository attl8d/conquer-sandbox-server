using Comet.Web;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Middlewares;
using Comet.Web.Model.Mvc;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Comet.Web.Services.Password;
using Comet.Web.Services.Realms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = true;
    opt.Password.RequiredLength = 6;
    opt.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    opt.Lockout.MaxFailedAccessAttempts = 5;
    opt.Lockout.AllowedForNewUsers = true;

    // User settings.
    opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    opt.User.RequireUniqueEmail = true;
    opt.SignIn.RequireConfirmedEmail = false;
    opt.SignIn.RequireConfirmedAccount = false;
    opt.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddRoles<ApplicationRole>()
    .AddUserManager<UserManager>()
    .AddRoleManager<RoleManager>()
    .AddUserStore<ApplicationUserStore>()
    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, int, ApplicationUserRole, ApplicationRoleClaim>>()
    .AddSignInManager<SignInManager>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = new PathString("/Login");
    opt.LogoutPath = new PathString("/Logout");
    opt.AccessDeniedPath = "/AccessDenied";

    opt.Cookie.SameSite = SameSiteMode.Strict;

    opt.SlidingExpiration = true;
});

Service.Register(builder.Services);

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(Security.SuperAdministrator, policy =>
    {
        policy.RequireRole(Security.SuperAdministrator);
#if !DEBUG
                    policy.RequireClaim("amr", "mfa");
#endif
    });
    opt.AddPolicy(Security.Administrator, policy => policy.RequireRole(Security.Administrator, Security.SuperAdministrator));
    opt.AddPolicy(Security.Moderator, policy => policy.RequireRole(Security.Moderator, Security.Administrator, Security.SuperAdministrator));
    opt.AddPolicy(Security.Member, policy => policy.RequireRole(Security.Member, Security.Moderator, Security.Administrator, Security.SuperAdministrator));
});

builder.Services.AddAuthentication()
    .AddFacebook(opt =>
    {
        opt.AppId = configuration["Authentication:Facebook:AppId"];
        opt.AppSecret = configuration["Authentication:Facebook:AppSecret"];
    })
    .AddGoogle(opt =>
    {
        opt.ClientId = configuration["Authentication:Google:ClientId"];
        opt.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    })
    .AddMicrosoftAccount(opt =>
    {
        opt.ClientId = configuration["Authentication:Microsoft:ClientId"];
        opt.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
    });

builder.Services.AddControllersWithViews(opt =>
{
    opt.UseGeneralRoutePrefix("api/v{version:apiVersion}");
});
builder.Services.AddApiVersioning(o => o.ReportApiVersions = true);

// Add services to the container.
#if DEBUG
//builder.Services.AddMvc().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "World Conquer Web API",
        Description = "Web API for querying data from game servers or web server.",
        Version = Constants.Version,
        Contact = new OpenApiContact
        {
            Email = "service@ftwmasters.com.br",
            Name = "Felipe Vieira Vendramini"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
#else
builder.Services.AddRazorPages();
builder.Services.AddMvc();
#endif

var app = builder.Build();


#if DEBUG
//if (env.IsDevelopment())
//{
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(opt =>
{
    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    opt.RoutePrefix = "swagger";

});
#else
app.UseExceptionHandler("/Error");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();
#endif

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseCometLocalization();
app.UseCometSession();
app.UseMaintenanceMiddleware();
app.UseBanMiddleware();

app.UseRequestLocalization(opt =>
{
    opt.DefaultRequestCulture = new RequestCulture(LanguageService.AvailableLanguages[0].LanguageCultureName);
    opt.SupportedCultures = LanguageService.AvailableLanguages.Select(x => new CultureInfo(x.LanguageCultureName)).ToList();
    opt.SupportedUICultures = LanguageService.AvailableLanguages.Select(x => new CultureInfo(x.LanguageCultureName)).ToList();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.Run();

using Comet.Web.App.Services.Identity;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Comet.Web.Services.Password;
using Comet.Web.Services.Realms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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

// Services that must be created every time
builder.Services.AddTransient<IEmailSender, EmailService>();
builder.Services.AddTransient<IPasswordHasher<ApplicationUser>, WhirlpoolPasswordService<ApplicationUser>>();
builder.Services.AddTransient<LanguageService>();
builder.Services.AddTransient<RealmService>();

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

// Add services to the container.
#if DEBUG
builder.Services.AddMvc().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddSwaggerGen();
#else
builder.Services.AddRazorPages();
builder.Services.AddMvc();
#endif

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

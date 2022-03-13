using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using Comet.Web.Model.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Services
{
    public class LanguageService
    {
        private Languages mCurrentCulture;
        private readonly IHttpContextAccessor mHttpContextAccessor;
        private readonly ILogger<LanguageService> mLogger;

        private readonly Dictionary<string, ResourceManager> ResourceManager = new();

        public LanguageService(ILogger<LanguageService> logger, IHttpContextAccessor httpContextAccessor)
        {
            mLogger = logger;
            mHttpContextAccessor = httpContextAccessor;

            Initialize();
        }

        public string CurrentLocale => CurrentCulture.LanguageCultureName;

        public Languages CurrentCulture
        {
            get
            {
                if (mCurrentCulture == null && mHttpContextAccessor != null)
                {
                    var locale = "";
                    if (!mHttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                    {
                        if (mHttpContextAccessor.HttpContext.Request.Cookies.ContainsKey("Locale"))
                            locale = mHttpContextAccessor.HttpContext.Request.Cookies["Locale"];
                        else
                            locale = mHttpContextAccessor.HttpContext.Request.Headers["Accept-Language"].ToString()
                                                         .Split(";")[0].Split(",")[0];
                    }
                    else
                    {
                        locale = mHttpContextAccessor.HttpContext.User.FindFirst(CustomClaimTypes.Language)?.Value;
                    }

                    if (!AvailableLanguages.Any(x => x.LanguageCultureName.Equals(locale)))
                        locale = GetDefaultLanguage();

                    mCurrentCulture = AvailableLanguages.FirstOrDefault(x => x.LanguageCultureName.Equals(locale));
                }
                else if (mCurrentCulture == null)
                {
                    mCurrentCulture =
                        AvailableLanguages.FirstOrDefault(x => x.LanguageCultureName.Equals(GetDefaultLanguage()));
                }

                return mCurrentCulture;
            }
        }

        private void Initialize()
        {
            const string location = "Comet.Web.Localization.{0}";
            ResourceManager.Add(
                "pt-BR", new ResourceManager(string.Format(location, "Language"), Assembly.GetExecutingAssembly()));
            foreach (Languages lang in AvailableLanguages)
                // mLogger.LogDebug($"Language {lang.LanguageFullName} has been loaded.");
                try
                {
                    if (!ResourceManager.ContainsKey(lang.LanguageCultureName))
                        ResourceManager.Add(lang.LanguageCultureName,
                                            new ResourceManager(
                                                string.Format($"{location}.Language", lang.LanguageCultureName),
                                                Assembly.GetExecutingAssembly()));
                }
                catch (Exception ex)
                {
                    mLogger.LogError(ex.ToString());
                }
        }

        public bool IsLanguageAvailable(string lang)
        {
            return AvailableLanguages.FirstOrDefault(a => a.LanguageCultureName.Equals(lang)) != null;
        }

        private static string GetDefaultLanguage()
        {
            return AvailableLanguages[0].LanguageCultureName;
        }

        public string GetString(string name, params object[] args)
        {
            string locale = CurrentCulture?.LanguageCultureName ?? GetDefaultLanguage();
            if (!ResourceManager.ContainsKey(locale))
                locale = GetDefaultLanguage();
            string result = ResourceManager[locale].GetString(name);
            if (string.IsNullOrEmpty(result)
                && string.IsNullOrEmpty(result = ResourceManager[GetDefaultLanguage()].GetString(name)))
                return name;
            return string.Format(result, args);
        }

        public static List<Languages> AvailableLanguages = new()
        {
            new() {LanguageFullName = "Português Brasileiro", LanguageCultureName = "pt-BR"},
            new() {LanguageFullName = "English", LanguageCultureName = "en-US"}
        };
    }

    public class Languages
    {
        public string LanguageFullName { get; set; }
        public string LanguageCultureName { get; set; }
        public string ImageUrl { get; set; }
        public bool Rtl { get; set; } = false;
    }
}
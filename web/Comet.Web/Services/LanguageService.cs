using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Claims;

namespace Comet.Web.Services
{
    public class LanguageService
    {
        public static List<Languages> AvailableLanguages = new List<Languages>
        {
            new Languages {LanguageFullName = "Português Brasileiro", LanguageCultureName = "pt-BR"},
            new Languages {LanguageFullName = "English", LanguageCultureName = "en-US"}
        };

        private Dictionary<string, ResourceManager> ResourceManager = new Dictionary<string, ResourceManager>();
        private ILogger<LanguageService> mLogger;
        private IHttpContextAccessor mHttpContextAccessor;

        private Languages mCurrentCulture = null;

        public LanguageService(ILogger<LanguageService> logger, IHttpContextAccessor httpContextAccessor)
        {
            mLogger = logger;
            mHttpContextAccessor = httpContextAccessor;

            Initialize();
        }

        public string CurrentLocale => CurrentCulture.LanguageCultureName;

        private void Initialize()
        {
            const string location = "Comet.Web.App.Localization.{0}";
            ResourceManager.Add("pt-BR", new ResourceManager(string.Format(location, "Language"), Assembly.GetExecutingAssembly()));
            foreach (var lang in AvailableLanguages)
            {
                mLogger.LogDebug($"Language {lang.LanguageFullName} has been loaded.");
                try
                {
                    if (!ResourceManager.ContainsKey(lang.LanguageCultureName))
                        ResourceManager.Add(lang.LanguageCultureName,
                            new ResourceManager(string.Format($"{location}.Language", lang.LanguageCultureName),
                                Assembly.GetExecutingAssembly()));
                }
                catch (Exception ex)
                {
                    mLogger.LogError(ex.ToString());
                }
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

        public Languages CurrentCulture
        {
            get
            {
                if (mCurrentCulture == null && mHttpContextAccessor != null)
                {
                    string locale = "";
                    if (!mHttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                    {
                        if (mHttpContextAccessor.HttpContext.Request.Cookies.ContainsKey("Locale"))
                        {
                            locale = mHttpContextAccessor.HttpContext.Request.Cookies["Locale"];
                        }
                        else
                        {
                            locale = mHttpContextAccessor.HttpContext.Request.Headers["Accept-Language"].ToString().Split(";")[0].Split(",")[0];
                        }
                    }
                    else
                    {
                        locale = mHttpContextAccessor.HttpContext.User.FindFirst("Language")?.Value;
                    }

                    if (!AvailableLanguages.Any(x => x.LanguageCultureName.Equals(locale)))
                        locale = GetDefaultLanguage();

                    mCurrentCulture = AvailableLanguages.FirstOrDefault(x => x.LanguageCultureName.Equals(locale));
                }
                else
                {
                    mCurrentCulture = AvailableLanguages.FirstOrDefault(x => x.LanguageCultureName.Equals(GetDefaultLanguage()));
                }
                return mCurrentCulture;
            }
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
    }

    public class Languages
    {
        public string LanguageFullName { get; set; }
        public string LanguageCultureName { get; set; }
        public string ImageUrl { get; set; }
        public bool Rtl { get; set; } = false;
    }
}

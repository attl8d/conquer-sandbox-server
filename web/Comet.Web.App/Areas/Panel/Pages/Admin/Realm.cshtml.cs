using Comet.Web.App.Services;
using Comet.Web.App.Services.Realms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Panel.Pages.Admin
{
    public class RealmModel : PageModel
    {
        private RealmService mRealmService;
        private LanguageService mLanguageService;

        public RealmModel(RealmService realmService, LanguageService language)
        {
            mRealmService = realmService;
            mLanguageService = language;
        }

        [BindProperty(SupportsGet = true)]
        public uint RealmID { get; set; }

        public RealmService.RealmPublicData Realm { get; set; }

        [BindProperty] public InputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await mRealmService.LoadRealmsAsync();

            if (RealmID == 0)
            {
                Input ??= new InputModel();
                return Page();
            }

            Realm = mRealmService.GetRealmData(RealmID);

            if (Realm.Equals(default))
            {
                return RedirectToPage("/Realms");
            }

            Input ??= new InputModel
            {
                DatabaseHost = Realm.DatabaseHostname,
                DatabasePort = (uint)Realm.DatabasePort,
                DatabaseSchema = Realm.DatabaseSchema,
                DatabaseUser = Realm.DatabaseUsername,

                Username = Realm.RealmUsername,

                GameIPAddress = Realm.GameIPAddress,
                GamePort = Realm.GamePort,

                RpcIPAddress = Realm.RpcIPAddress,
                RpcPort = Realm.RpcPort,

                RealmID = Realm.RealmID,
                RealmName = Realm.RealmName
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await mRealmService.LoadRealmsAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (RealmID == 0)
            {
                if (string.IsNullOrEmpty(Input.DatabasePass) || string.IsNullOrEmpty(Input.Password))
                {
                    ModelState.AddModelError("PasswordError", "PasswordsCantBeEmptyOnCreation");
                }

                if (!await mRealmService.CreateRealmAsync(new RealmService.CreateRealmModel
                {
                    DatabaseHost = Input.DatabaseHost,
                    DatabasePass = Input.DatabasePass,
                    DatabasePort = Input.DatabasePort.ToString(),
                    DatabaseSchema = Input.DatabaseSchema,
                    DatabaseUser = Input.DatabaseUser,

                    GameIPAddress = Input.GameIPAddress,
                    GamePort = Input.GamePort,
                    RpcIPAddress = Input.RpcIPAddress,
                    RpcPort = Input.RpcPort,

                    RealmName = Input.RealmName,
                    Username = Input.Username,
                    Password = Input.Password
                }))
                {
                    ModelState.AddModelError("DbError", mLanguageService.GetString("CouldNotInsertRealm"));
                    return Page();
                }

                return RedirectToPage("/Realms");
            }
            else
            {
                if(!await mRealmService.UpdateRealmAsync(new RealmService.AlterRealmModel
                {
                    DatabaseHost = Input.DatabaseHost,
                    DatabasePass = Input.DatabasePass,
                    DatabasePort = Input.DatabasePort.ToString(),
                    DatabaseSchema = Input.DatabaseSchema,
                    DatabaseUser = Input.DatabaseUser,

                    GameIPAddress = Input.GameIPAddress,
                    GamePort = Input.GamePort,
                    RpcIPAddress = Input.RpcIPAddress,
                    RpcPort = Input.RpcPort,

                    RealmName = Input.RealmName,
                    Username = Input.Username,
                    Password = Input.Password,
                    RealmID = RealmID
                }))
                {
                    ModelState.AddModelError("DbError", mLanguageService.GetString("CouldNotUpdateRealm"));
                    return Page();
                }
            }

            return RedirectToPage();
        }

        public class InputModel
        {
            public uint RealmID { get; set; }
            [Required]
            [MinLength(4)]
            [MaxLength(16)]
            public string RealmName { get; set; }
            [Required]
            [MaxLength(16)]
            public string GameIPAddress { get; set; }
            [Required]
            [MaxLength(16)]
            public string RpcIPAddress { get; set; } = "127.0.0.1";
            [Required]
            public uint GamePort { get; set; } = 5816;
            [Required]
            public uint RpcPort { get; set; } = 5817;
            [Required]
            [MaxLength(16)]
            public string Username { get; set; }
            [MaxLength(16)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            public string DatabaseHost { get; set; } = "localhost";
            [Required]
            public string DatabaseUser { get; set; } = "root";
#if !DEBUG
            [MinLength(20)]
            [MaxLength(100)]
#endif
            [DataType(DataType.Password)]
            public string DatabasePass { get; set; }
            [Required]
            public string DatabaseSchema { get; set; } = "cq";
            [Required]
            public uint DatabasePort { get; set; } = 3306;
        }
    }
}

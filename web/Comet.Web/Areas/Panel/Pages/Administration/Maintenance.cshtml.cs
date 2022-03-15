using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Comet.Shared;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Panel.Pages.Administration
{
    [Authorize(Policy = Security.SuperAdministrator)]
    public class MaintenanceModel : PageModel
    {
        private readonly MaintenanceService mService;
        private readonly LanguageService mLanguage;

        public MaintenanceModel(MaintenanceService service, LanguageService language)
        {
            mService = service;
            mLanguage = language;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Input == null)
            {
                SystemConfig element = await mService.GetAsync();
                Input = new InputModel
                {
                    MaintenanceActive = element.Data0 != 0,
                    StartDate = element.Data1 == 0 ? null : UnixTimestamp.ToDateTime((uint) element.Data1),
                    EndDate = element.Data2 == 0 ? null : UnixTimestamp.ToDateTime((uint) element.Data2),
                    Message = element.Text
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            SystemConfig config = await mService.GetAsync();
            if (Input.StartDate != null && Input.EndDate != null)
            {
                if (Input.StartDate > Input.EndDate)
                {
                    ModelState.AddModelError("InvalidStartDate", mLanguage.GetString("InvalidStartDate"));
                    return Page();
                }

                if (Input.EndDate < Input.StartDate)
                {
                    ModelState.AddModelError("InvalidEndDate", mLanguage.GetString("InvalidEndDate"));
                    return Page();
                }
            }

            if (string.IsNullOrEmpty(Input.Message))
            {
                ModelState.AddModelError("MaintenanceShouldHaveDescription", mLanguage.GetString("MaintenanceShouldHaveDescription"));
                return Page();
            }

            config.Data0 = Input.MaintenanceActive ? 1 : 0;
            config.Data1 = Input.StartDate.HasValue ? UnixTimestamp.Timestamp(Input.StartDate.Value) : 0;
            config.Data2 = Input.EndDate.HasValue ? UnixTimestamp.Timestamp(Input.EndDate.Value) : 0;
            config.Text = Input.Message;

            if (!await mService.SaveAsync(config))
            {
                ModelState.AddModelError("InternalServerError", mLanguage.GetString("InternalServerError"));
                return Page();
            }

            return Page();
        }

        public class InputModel
        {
            public bool MaintenanceActive { get; set; }
            [DataType(DataType.DateTime)]
            public DateTime? StartDate { get; set; }
            [DataType(DataType.DateTime)]
            public DateTime? EndDate { get; set; }
            [Required]
            [MaxLength(4096)]
            public string Message { get; set; }
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Comet.Web.App.API
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

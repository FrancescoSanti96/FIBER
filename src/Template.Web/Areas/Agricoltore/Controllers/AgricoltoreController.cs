using Microsoft.AspNetCore.Mvc;
using Template.Web.Areas;

namespace Template.Web.Areas.Agricoltore.Controllers
{
    [Area("Agricoltore")]
    public partial class AgricoltoreController : AuthenticatedBaseController
    {
        public virtual IActionResult BollettiniAgricoltore()
        {
            return View();
        }

        public virtual IActionResult ImpostazioniAgricoltore()
        {
            return View();
        }

        public virtual IActionResult Bollettino()
        {
            return View();
        }
    }
} 
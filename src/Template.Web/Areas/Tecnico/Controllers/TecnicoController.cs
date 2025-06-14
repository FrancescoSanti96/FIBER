using Microsoft.AspNetCore.Mvc;
using Template.Web.Areas;
using Template.Web.Models;

namespace Template.Web.Areas.Tecnico.Controllers
{
    [Area("Tecnico")]
    public partial class TecnicoController : AuthenticatedBaseController
    {
        // GET: Tecnico/Tecnico/HomeTecnico
        public virtual IActionResult HomeTecnico(string tab = "caricati")
        {
            ViewBag.ActiveTab = tab;
            return View();
        }

        // GET: Tecnico/Tecnico/BollettiniCaricati
        public virtual IActionResult BollettiniCaricati()
        {
            return View("HomeTecnico");
        }

        // GET: Tecnico/Tecnico/Bozze
        public virtual IActionResult Bozze()
        {
            return View("HomeTecnico");
        }

        // GET: Tecnico/Tecnico/BollettinoTecnico
        public virtual IActionResult BollettinoTecnico(int id, bool isDraft = false)
        {
            // In un'applicazione reale, qui recupereresti i dati del bollettino dal database
            // usando l'id e creeresti un ViewModel più completo.
            var model = new BollettinoTecnicoViewModel
            {
                IsDraft = isDraft
                // Popola altre proprietà del bollettino qui
            };
            return View(model);
        }

        // GET: Tecnico/Tecnico/ImpostazioniTecnico
        public virtual IActionResult ImpostazioniTecnico()
        {
            return View();
        }
    }
} 
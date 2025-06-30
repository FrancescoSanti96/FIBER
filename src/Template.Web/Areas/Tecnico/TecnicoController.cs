using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Template.Services.Coltures;
using Template.Services.Provinces;
using Template.Services.Users;
using Template.Web.Areas.Tecnico.ViewModels;
using Template.Web.Models;

namespace Template.Web.Areas.Tecnico
{
    [Area("Tecnico")]
    public class TecnicoController : AuthenticatedBaseController
    {
        private readonly ColtureService _coltureService;
        private readonly ProvinceService _provinceService;

        public TecnicoController(UserService userService,
            ColtureService coltureService,
            ProvinceService provinceService)
            : base(userService)
        {
            _coltureService = coltureService;
            _provinceService = provinceService;
        }

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

        // GET: Tecnico/Tecnico/NuovoBollettino
        public async Task<IActionResult> NuovoBollettino()
        {
            var vm = new NuovoBollettinoViewModel()
            {
                OpzioniColture = [.. (await _coltureService.Query()).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),

                })],
                OpzioniProvince = [.. (await _provinceService.Query()).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),

                })]
            };

            return View(vm);
        }

        // GET: Tecnico/Tecnico/ModificaBollettino
        public virtual IActionResult ModificaBollettino(int id)
        {
            // In un'applicazione reale, qui recupereresti i dati del bollettino dal database
            // usando l'id e creeresti un ViewModel più completo.
            var model = new ModificaBollettinoViewModel
            {
                // Esempio: recupera il bollettino con l'ID fornito
                // var bollettino = _bollettiniService.GetBollettinoById(id);
                // Title = bollettino.Title,
                // Content = bollettino.Content,
                // NomeBollettino = bollettino.NomeBollettino,
                // CulturaInteresse = bollettino.Cultura,
                // ZonaInteresse = bollettino.Zona,
                // ScadenzaTemporale = bollettino.Scadenza.ToString("dd/MM/yyyy")
                Title = $"Modifica Bollettino {id}", // Dati di esempio per ora
                Content = "Contenuto di esempio per il bollettino da modificare.",
                NomeBollettino = "Bollettino di Esempio",
                CulturaInteresse = "Grano",
                ZonaInteresse = "Pianura Padana",
                ScadenzaTemporale = "01/01/2026"
            };
            return View(model);
        }
    }
}
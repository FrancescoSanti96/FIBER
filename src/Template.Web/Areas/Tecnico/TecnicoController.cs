using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Template.EntityModel.Models;
using Template.Services.Coltures;
using Template.Services.Provinces;
using Template.Services.Users;
using Template.Web.Areas.Tecnico.ViewModels;
using Template.Web.Infrastructure;
using Template.Web.Models;
using Template.Web.Services;

namespace Template.Web.Areas.Tecnico
{
    [Area("Tecnico")]
    public class TecnicoController : AuthenticatedBaseController
    {
        private readonly ColtureService _coltureService;
        private readonly ProvinceService _provinceService;
        private readonly IPdfService _pdfService;

        public TecnicoController(UserService userService,
            ColtureService coltureService,
            ProvinceService provinceService,
            IPdfService pdfService)
            : base(userService)
        {
            _coltureService = coltureService;
            _provinceService = provinceService;
            _pdfService = pdfService;
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
        public virtual async Task<IActionResult> BollettinoTecnico(int id = 3, bool isDraft = false)
        {
            // TODO: MOCK - Sostituire con recupero dati reale dal database
            // Per attivare la chiamata reale, decommentare il blocco qui sotto e rimuovere i dati mock
            /*
            try
            {
                var bulletinDto = await _userService.Query(new GetBulletinByIdQuery { Id = id });
                
                if (bulletinDto == null)
                {
                    Alerts.AddError(this, "Bollettino non trovato");
                    return RedirectToAction(nameof(BollettiniCaricati));
                }

                var model = new BollettinoTecnicoViewModel
                {
                    Id = bulletinDto.Id,
                    Titolo = bulletinDto.Summary ?? "Bollettino senza titolo",
                    ContenutoHTML = bulletinDto.Body ?? "",
                    AutoreNome = bulletinDto.AuthorFirstName,
                    AutoreCognome = bulletinDto.AuthorLastName,
                    AutoreEmail = bulletinDto.AuthorEmail,
                    DataPubblicazione = bulletinDto.PublishDate,
                    DataScadenza = bulletinDto.ExpireDate,
                    Province = bulletinDto.ProvinceNames,
                    Colture = bulletinDto.ColtureNames,
                    Pubblicato = bulletinDto.Published,
                    IsDraft = !bulletinDto.Published
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, $"Errore nel caricamento del bollettino: {ex.Message}");
                return RedirectToAction(nameof(BollettiniCaricati));
            }
            */

            // DATI MOCK - Bollettino ID 5 dal database
            var model = new BollettinoTecnicoViewModel
            {
                Id = 5,
                Titolo = "Albicocco, Carota da seme - Ferrara",
                ContenutoHTML = "	<h2>Situazione</h2><p><br><strong>Le condizioni climatiche delle ultime settimane </strong>hanno favorito lo sviluppo di infezioni fungine, in particolare di peronospora. Si osservano i primi sintomi su foglia in diverse zone del territorio.<br>Previsioni meteo<br>&nbsp;</p><ul><li>Per i prossimi giorni si prevedono <strong>temperature</strong> in aumento e precipitazioni sparse, condizioni che potrebbero favorire lo sviluppo di infezioni secondarie.</li><li>Peronospora<br>Si consiglia di <strong>monitorare</strong> attentamente i vigneti. In presenza di sintomi, intervenire con prodotti a base di rame o altri fungicidi specifici. Nelle zone a maggior rischio, si consiglia un trattamento preventivo.<br><strong>Oidio</strong></li><li>Il rischio di infezioni è moderato. Si consiglia di intervenire con zolfo nelle ore più fresche della giornata, evitando le ore più calde.<br><br><strong>Tignoletta</strong><br><i>Il monitoraggio con trappole a feromoni indica un aumento delle catture. Si prevede l'inizio dei voli della prima generazione. In caso di superamento della soglia di intervento, si consiglia di intervenire con prodotti specifici..</i></li></ul><p>&nbsp;</p><ol><li><i>test</i></li><li><i>test</i></li><li><i>test</i></li></ol>",
                AutoreNome = "Marco",
                AutoreCognome = "Romano",
                AutoreEmail = "marco.romano@agricoltura.com",
                DataPubblicazione = DateTime.Parse("2025-07-02 21:26:02.754169"),
                DataScadenza = DateOnly.Parse("2025-07-10"),
                Province = ["Ferrara"], // Dati mock
                Colture = ["Albicocco", "Carota da seme"], // Dati mock
                Pubblicato = true,
                IsDraft = false
            };

            return View(model);
        }

        // GET: Tecnico/Tecnico/NuovoBollettino
        public async Task<IActionResult> NuovoBollettino() =>
          View(await GetNuovoBollettinoViewModel());

        [HttpPost]
        public async Task<IActionResult> NuovoBollettino(NuovoBollettinoViewModel model)
        {
            var baseVm = await GetNuovoBollettinoViewModel();
            model.OpzioniColture = baseVm.OpzioniColture;
            model.OpzioniProvince = baseVm.OpzioniProvince;
            try
            {
                if (model.Pubblicato is true && !ModelState.IsValid)
                {
                    var validationMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    Alerts.AddError(this, string.Join(", ", validationMessages));
                    return View(model);
                }
                else
                {
                    var currentUser = await GetCurrentUserAsync()
                       ?? throw new InvalidOperationException("Utente non trovato");

                    var bulletin = new Bulletin
                    {
                        IdUser = currentUser.Id,
                        Summary = model.TitoloBollettino,
                        Body = model.ContenutoBollettino,
                        Published = model.Pubblicato,
                        ExpireDate = model.Scadenza
                    };

                    var newBulletinId = await _userService.AddNewBulletinAsync(bulletin, provinceIds: model.IdProvinceSelezionate, coltureIds: model.IdColtureSelezionate);
                    return RedirectToAction(nameof(BollettinoTecnico), new { Id = newBulletinId });
                }
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, ex.Message);
                return View(model);
            }
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

        // GET: Tecnico/Tecnico/DownloadBollettino
        public virtual IActionResult DownloadBollettino(int id = 5)
        {
            try
            {
                // DATI MOCK - Bollettino ID 4 dal database
                var title = "Albicocco, Carciofo, Ciliegio - Ferrara, Forlì-Cesena";
                var content = "	<h2>Situazione</h2><p><br><strong>Le condizioni climatiche delle ultime settimane </strong>hanno favorito lo sviluppo di infezioni fungine, in particolare di peronospora. Si osservano i primi sintomi su foglia in diverse zone del territorio.<br>Previsioni meteo<br>&nbsp;</p><ul><li>Per i prossimi giorni si prevedono <strong>temperature</strong> in aumento e precipitazioni sparse, condizioni che potrebbero favorire lo sviluppo di infezioni secondarie.</li><li>Peronospora<br>Si consiglia di <strong>monitorare</strong> attentamente i vigneti. In presenza di sintomi, intervenire con prodotti a base di rame o altri fungicidi specifici. Nelle zone a maggior rischio, si consiglia un trattamento preventivo.<br><strong>Oidio</strong></li><li>Il rischio di infezioni è moderato. Si consiglia di intervenire con zolfo nelle ore più fresche della giornata, evitando le ore più calde.<br><br><strong>Tignoletta</strong><br><i>Il monitoraggio con trappole a feromoni indica un aumento delle catture. Si prevede l'inizio dei voli della prima generazione. In caso di superamento della soglia di intervento, si consiglia di intervenire con prodotti specifici..</i></li></ul><p>&nbsp;</p><ol><li><i>test</i></li><li><i>test</i></li><li><i>test</i></li></ol>";
                var author = "Marco Romano";
                var publishDate = DateTime.Parse("2025-07-02 21:26:02.754169");
                var expireDate = DateOnly.Parse("2025-07-10");
                var provinces = new List<string> { "Ferrara", "Forlì-Cesena" };
                var coltures = new List<string> { "Albicocco", "Carciofo", "Ciliegio" };

                Console.WriteLine($"Generazione PDF per bollettino {id}");
                Console.WriteLine($"Titolo: {title}");
                Console.WriteLine($"Contenuto: {content}");
                Console.WriteLine($"Autore: {author}");

                var pdfBytes = _pdfService.GenerateBulletinPdf(title, content, author, publishDate, expireDate, provinces, coltures);

                Console.WriteLine($"PDF generato con successo. Dimensione: {pdfBytes.Length} bytes");

                var fileName = $"bollettino_{id}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRORE nella generazione PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Alerts.AddError(this, $"Errore nella generazione del PDF: {ex.Message}");
                return RedirectToAction(nameof(BollettinoTecnico), new { id });
            }
        }
        #region Private methods
        private async Task<NuovoBollettinoViewModel> GetNuovoBollettinoViewModel()
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

            return vm;
        }
        #endregion
    }

}
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Template.Services.Users;
using Template.Web.Areas.Dto;
using Template.Web.Models;
using Template.Web.Infrastructure;
using Template.Web.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Collections.Generic;
using Template.Web.Areas.Agricoltore.Dto;
using Template.Web.Areas.Agricoltore.ViewModels;
using Template.Services.Bulletins;
using Template.Web.Models;

namespace Template.Web.Areas.Agricoltore
{
    [Area("Agricoltore")]
    public class AgricoltoreController : AuthenticatedBaseController
    {
        private readonly ILogger<AgricoltoreController> _logger;
        private readonly IPdfService _pdfService;

        public AgricoltoreController(UserService userService, ILogger<AgricoltoreController> logger, IPdfService pdfService) : base(userService)
        {
            _logger = logger;
            _pdfService = pdfService;
        private readonly BollettiniService _bollettiniService;

        public AgricoltoreController(UserService userService, 
            ILogger<AgricoltoreController> logger,
            BollettiniService bollettiniService) : base(userService)
        {
            _logger = logger;
            _bollettiniService = bollettiniService;
        }

        public async Task<IActionResult> BollettiniAgricoltore(BollettiniAgricoltoreViewModel vm)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound("Invalid user");
            }

            // Controlla se l'agricoltore ha completato l'onboarding
            if (!user.OnboardingComplete)
            {
                // Reindirizza all'onboarding se non l'ha completato
                return RedirectToAction("OnboardingStep1", "Agricoltore", new { area = "Agricoltore" });
            }

            var query = new BollettiniListQuery
            {
                FilterExpression = x => x.Published == true,
                Paging = new Template.Infrastructure.Paging
                {
                    OrderBy = vm.OrderBy,
                    OrderByDescending = vm.OrderByDescending,
                    Page = vm.Page,
                    PageSize = vm.PageSize
                }
            };

            var bollettini = (await _bollettiniService.Query(query)).Select(x => new BollettinoCardViewModel(x)).ToList();

            vm.Bollettini = bollettini;
            vm.TotalItems = bollettini.Count;

            return View(vm);
        }

        public virtual async Task<IActionResult> ImpostazioniAgricoltore()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound("Invalid user");
            }

            // Controlla se l'agricoltore ha completato l'onboarding
            if (!user.OnboardingComplete)
            {
                // Reindirizza all'onboarding se non l'ha completato
                return RedirectToAction("OnboardingStep1", "Agricoltore", new { area = "Agricoltore" });
            }

            return View();
        }

        // GET: Agricoltore/Agricoltore/Bollettino
        public virtual async Task<IActionResult> Bollettino(int id = 3)
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
                    return RedirectToAction(nameof(BollettiniAgricoltore));
                }
                var model = new BollettinoAgricoltoreViewModel
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
                    Pubblicato = bulletinDto.Published
                };
                return View(model);
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, $"Errore nel caricamento del bollettino: {ex.Message}");
                return RedirectToAction(nameof(BollettiniAgricoltore));
            }
            */

            // DATI MOCK - Bollettino ID 5 dal database (pubblicato)
            var model = new BollettinoAgricoltoreViewModel
            {
                Id = 5,
                Titolo = "Albicocco, Carota da seme - Ferrara",
                ContenutoHTML = "<h2>Situazione</h2><p><br><strong>Le condizioni climatiche delle ultime settimane </strong>hanno favorito lo sviluppo di infezioni fungine, in particolare di peronospora. Si osservano i primi sintomi su foglia in diverse zone del territorio.<br>Previsioni meteo<br>&nbsp;</p><ul><li>Per i prossimi giorni si prevedono <strong>temperature</strong> in aumento e precipitazioni sparse, condizioni che potrebbero favorire lo sviluppo di infezioni secondarie.</li><li>Peronospora<br>Si consiglia di <strong>monitorare</strong> attentamente i vigneti. In presenza di sintomi, intervenire con prodotti a base di rame o altri fungicidi specifici. Nelle zone a maggior rischio, si consiglia un trattamento preventivo.<br><strong>Oidio</strong></li><li>Il rischio di infezioni è moderato. Si consiglia di intervenire con zolfo nelle ore più fresche della giornata, evitando le ore più calde.<br><br><strong>Tignoletta</strong><br><i>Il monitoraggio con trappole a feromoni indica un aumento delle catture. Si prevede l'inizio dei voli della prima generazione. In caso di superamento della soglia di intervento, si consiglia di intervenire con prodotti specifici..</i></li></ul><p>&nbsp;</p><ol><li><i>test</i></li><li><i>test</i></li><li><i>test</i></li></ol>",
                AutoreNome = "Marco",
                AutoreCognome = "Romano",
                AutoreEmail = "marco.romano@agricoltura.com",
                DataPubblicazione = DateTime.Parse("2025-07-02 21:26:02.754169"),
                DataScadenza = DateOnly.Parse("2025-07-10"),
                Province = ["Ferrara"], // Dati mock
                Colture = ["Albicocco", "Carota da seme"], // Dati mock
                Pubblicato = true
            };

            return View(model);
        }

        // GET: Agricoltore/Agricoltore/DownloadBollettino
        public virtual IActionResult DownloadBollettino(int id = 5)
        {
            try
            {
                // DATI MOCK - Bollettino ID 5 dal database (pubblicato)
                var title = "Albicocco, Carota da seme - Ferrara";
                var content = "<h2>Situazione</h2><p><br><strong>Le condizioni climatiche delle ultime settimane </strong>hanno favorito lo sviluppo di infezioni fungine, in particolare di peronospora. Si osservano i primi sintomi su foglia in diverse zone del territorio.<br>Previsioni meteo<br>&nbsp;</p><ul><li>Per i prossimi giorni si prevedono <strong>temperature</strong> in aumento e precipitazioni sparse, condizioni che potrebbero favorire lo sviluppo di infezioni secondarie.</li><li>Peronospora<br>Si consiglia di <strong>monitorare</strong> attentamente i vigneti. In presenza di sintomi, intervenire con prodotti a base di rame o altri fungicidi specifici. Nelle zone a maggior rischio, si consiglia un trattamento preventivo.<br><strong>Oidio</strong></li><li>Il rischio di infezioni è moderato. Si consiglia di intervenire con zolfo nelle ore più fresche della giornata, evitando le ore più calde.<br><br><strong>Tignoletta</strong><br><i>Il monitoraggio con trappole a feromoni indica un aumento delle catture. Si prevede l'inizio dei voli della prima generazione. In caso di superamento della soglia di intervento, si consiglia di intervenire con prodotti specifici..</i></li></ul><p>&nbsp;</p><ol><li><i>test</i></li><li><i>test</i></li><li><i>test</i></li></ol>";
                var author = "Marco Romano";
                var publishDate = DateTime.Parse("2025-07-02 21:26:02.754169");
                var expireDate = DateOnly.Parse("2025-07-10");
                var provinces = new List<string> { "Ferrara" };
                var coltures = new List<string> { "Albicocco", "Carota da seme" };

                Console.WriteLine("CONTENUTO HTML PASSATO AL PDF:\n" + content);

                var pdfBytes = _pdfService.GenerateBulletinPdf(title, content, author, publishDate, expireDate, provinces, coltures);

                var fileName = $"bollettino_{id}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella generazione del PDF per il bollettino {Id}", id);
                Alerts.AddError(this, "Errore nella generazione del PDF");
                return RedirectToAction(nameof(Bollettino), new { id });
            }
        }

        // API endpoint per caricare le province
        [HttpGet]
        public virtual async Task<IActionResult> GetProvinces()
        {
            try
            {
                var provinces = await _userService.Query(new GetAllProvincesQuery());
                var provinceDtos = provinces.Select(p => new { id = p.Id, name = p.Name }).ToList();
                return Json(new { success = true, data = provinceDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento delle province");
                return Json(new { success = false, message = "Errore nel caricamento delle province" });
            }
        }

        // API endpoint per caricare le colture
        [HttpGet]
        public virtual async Task<IActionResult> GetColtures()
        {
            try
            {
                var coltures = await _userService.Query(new GetAllColturesQuery());
                var coltureDtos = coltures.Select(c => new { id = c.Id, name = c.Name }).ToList();
                return Json(new { success = true, data = coltureDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento delle colture");
                return Json(new { success = false, message = "Errore nel caricamento delle colture" });
            }
        }

        // API endpoint per caricare le preferenze dell'utente
        [HttpGet]
        public virtual async Task<IActionResult> GetUserPreferences()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Json(new { success = false, message = "Utente non trovato" });
                }

                // Recupera l'utente completo con le relazioni
                var userWithPreferences = await _userService.Query(new GetUserWithPreferencesQuery { Id = user.Id });

                if (userWithPreferences == null)
                {
                    return Json(new { success = false, message = "Utente non trovato" });
                }

                var selectedProvinces = userWithPreferences.Provinces?.Select(p => p.Id).ToList() ?? new List<int>();
                var selectedColtures = userWithPreferences.Coltures?.Select(c => c.Id).ToList() ?? new List<int>();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        provinces = selectedProvinces,
                        coltures = selectedColtures
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento delle preferenze utente");
                return Json(new { success = false, message = "Errore nel caricamento delle preferenze" });
            }
        }

        // Onboarding Step 1 - Zone di interesse
        public virtual async Task<IActionResult> OnboardingStep1()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound("Invalid user");
            }

            // Solo gli agricoltori (RoleId = 2) possono fare l'onboarding
            if (user.RoleId != 2)
            {
                return RedirectToAction("BollettiniAgricoltore", "Agricoltore", new { area = "Agricoltore" });
            }

            return View();
        }

        // Onboarding Step 2 - Colture di interesse
        public virtual async Task<IActionResult> OnboardingStep2()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound("Invalid user");
            }

            // Solo gli agricoltori (RoleId = 2) possono fare l'onboarding
            if (user.RoleId != 2)
            {
                return RedirectToAction("BollettiniAgricoltore", "Agricoltore", new { area = "Agricoltore" });
            }

            return View();
        }

        // Salva le preferenze dell'onboarding
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> SaveOnboardingPreferences([FromBody] SavePreferencesDto dto)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return NotFound("Invalid user");
            }

            // Solo gli agricoltori (RoleId = 2) possono salvare le preferenze dell'onboarding
            if (user.RoleId != 2)
            {
                return Json(new { success = false, message = "Solo gli agricoltori possono completare l'onboarding" });
            }

            var saved = await _userService.SaveUserSettingsAsync(user.Id, dto.Coltures, dto.Provinces);

            // Dopo aver salvato le preferenze, reindirizza alla home dell'agricoltore
            return Json(new { success = true, redirectUrl = Url.Action("BollettiniAgricoltore", "Agricoltore", new { area = "Agricoltore" }) });
        }

        public virtual async Task<IActionResult> SaveUserPreferences(SavePreferencesDto dto)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return NotFound("Invalid user");
            }

            var saved = await _userService.SaveUserSettingsAsync(user.Id, dto.Coltures, dto.Provinces);

            return RedirectToAction("ImpostazioniAgricoltore", "Agricoltore", new { area = "Agricoltore" });
        }

        // API endpoint per salvare le preferenze dell'utente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> SaveUserPreferencesAjax([FromBody] SavePreferencesDto dto)
        {
            try
            {
                var user = await GetCurrentUserAsync();

                if (user == null)
                {
                    return Json(new { success = false, message = "Utente non trovato" });
                }

                var saved = await _userService.SaveUserSettingsAsync(user.Id, dto.Coltures, dto.Provinces);

                return Json(new { success = true, message = "Preferenze salvate con successo!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel salvataggio delle preferenze utente");
                return Json(new { success = false, message = "Errore nel salvataggio delle preferenze" });
            }
        }
    }
}
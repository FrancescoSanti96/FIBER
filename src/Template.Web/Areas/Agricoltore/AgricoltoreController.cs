using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Template.Services.Users;
using Template.Services.Bulletins;
using Template.Web.Areas.Agricoltore.Dto;
using Template.Web.Areas.Agricoltore.ViewModels;
using Template.Web.Models;
using Template.Web.Infrastructure;
using Template.Web.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using Template.Services.Coltures;
using Template.Services.Provinces;

namespace Template.Web.Areas.Agricoltore
{
    [Area("Agricoltore")]
    public class AgricoltoreController : AuthenticatedBaseController
    {
        private readonly ILogger<AgricoltoreController> _logger;
        private readonly IPdfService _pdfService;
        private readonly BollettiniService _bollettiniService;
        private readonly ColtureService _coltureService;
        private readonly ProvinceService _provinceService;

        public AgricoltoreController(UserService userService, 
            ILogger<AgricoltoreController> logger,
            IPdfService pdfService,
            BollettiniService bollettiniService,
            ColtureService coltureService,
            ProvinceService provinceService) : base(userService)
        {
            _logger = logger;
            _pdfService = pdfService;
            _bollettiniService = bollettiniService;
            _coltureService = coltureService;
            _provinceService = provinceService;
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

            // Controlla se l'onboarding è appena stato completato e mostra la notifica
            if (TempData["OnboardingCompleted"] != null && (bool)TempData["OnboardingCompleted"])
            {
                Alerts.AddSuccess(this, "🎉 Benvenuto! Il tuo profilo è stato configurato con successo. Ora puoi visualizzare bollettini personalizzati per le tue colture e zone di interesse.", 6000);
            }

            var userWithPreferences = await _userService.Query(new GetUserWithPreferencesQuery { Id = user.Id });
            var queryDto = new BulletinListQuery
            {
                FilterExpression = x => x.Published == true,
                Paging = new Template.Infrastructure.Paging
                {
                    OrderBy = vm.OrderBy,
                    OrderByDescending = vm.OrderByDescending,
                    Page = vm.Page,
                    PageSize = vm.PageSize
                },
                ColtureFilter = [.. userWithPreferences.Coltures.Select(c => c.Id)],
                ProvinceFilter = [.. userWithPreferences.Provinces.Select(p => p.Id)]
            };

            var bulletins = await _bollettiniService.Query(queryDto);
            vm.SetBollettini(bulletins);

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

            // Controlla se le impostazioni sono state appena aggiornate e mostra la notifica
            if (TempData["SettingsUpdated"] != null && (bool)TempData["SettingsUpdated"])
            {
                Alerts.AddSuccess(this, "Preferenze aggiornate con successo! I tuoi bollettini personalizzati sono ora disponibili.", 5000);
            }

            return View();
        }

        // GET: Agricoltore/Agricoltore/Bollettino
        public virtual async Task<IActionResult> Bollettino(int id)
        {
            try
            {
                var bulletinDto = await _bollettiniService.Query(new GetBulletinByIdQuery { Id = id });
                
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
                    Province = bulletinDto.Provinces,
                    Colture = bulletinDto.Coltures,
                    Pubblicato = bulletinDto.Published
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, $"Errore nel caricamento del bollettino: {ex.Message}");
                return RedirectToAction(nameof(BollettiniAgricoltore));
            }
        }

        // GET: Agricoltore/Agricoltore/DownloadBollettino
        public virtual async Task<IActionResult> DownloadBollettino(int id)
        {
            try
            {
                var bulletinDto = await _bollettiniService.Query(new GetBulletinByIdQuery { Id = id });
                
                if (bulletinDto == null)
                {
                    Alerts.AddError(this, "Bollettino non trovato");
                    return RedirectToAction(nameof(BollettiniAgricoltore));
                }

                var title = bulletinDto.Summary ?? "Bollettino senza titolo";
                var content = bulletinDto.Body ?? "";
                var author = $"{bulletinDto.AuthorFirstName} {bulletinDto.AuthorLastName}";
                var publishDate = bulletinDto.PublishDate ?? DateTime.Now;
                var expireDate = bulletinDto.ExpireDate;
                var provinces = bulletinDto.Provinces?.ToList() ?? new List<string>();
                var coltures = bulletinDto.Coltures?.ToList() ?? new List<string>();


                var pdfBytes = _pdfService.GenerateBulletinPdf(title, content, author, publishDate, expireDate, provinces, coltures);

                var fileName = $"bollettino_{id}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRORE nella generazione PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Alerts.AddError(this, $"Errore nella generazione del PDF: {ex.Message}");
                return RedirectToAction(nameof(BollettiniAgricoltore));
            }
        }

        // API endpoint per caricare le province
        [HttpGet]
        public virtual async Task<IActionResult> GetProvinces()
        {
            try
            {
                var provinces = await _provinceService.Query();
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
                var coltures = await _coltureService.Query();
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

            // Usa TempData per passare la notifica attraverso il redirect JavaScript
            TempData["OnboardingCompleted"] = true;

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

            // Aggiungi notifica di successo per il salvataggio delle impostazioni
            Alerts.AddSuccess(this, "Le tue preferenze sono state aggiornate con successo! I bollettini mostrati saranno personalizzati in base alle tue nuove impostazioni.", 5000);

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

                // Usa TempData per passare la notifica attraverso il redirect JavaScript
                TempData["SettingsUpdated"] = true;

                return Json(new { success = true, redirect = true, url = Url.Action("ImpostazioniAgricoltore", "Agricoltore", new { area = "Agricoltore" }) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel salvataggio delle preferenze utente");
                return Json(new { success = false, message = "Errore nel salvataggio delle preferenze" });
            }
        }
    }
}
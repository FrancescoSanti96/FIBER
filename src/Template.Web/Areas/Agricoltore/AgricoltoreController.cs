using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Template.Services.Users;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Collections.Generic;
using Template.Web.Areas.Agricoltore.Dto;
using Template.Web.Areas.Agricoltore.ViewModels;
using Template.Services.Bulletins;
using Template.Web.Models;
using Template.Services.Coltures;
using Template.Services.Provinces;

namespace Template.Web.Areas.Agricoltore
{
    [Area("Agricoltore")]
    public class AgricoltoreController : AuthenticatedBaseController
    {
        private readonly ILogger<AgricoltoreController> _logger;
        private readonly BollettiniService _bollettiniService;
        private readonly ColtureService _coltureService;
        private readonly ProvinceService _provinceService;

        public AgricoltoreController(UserService userService, 
            ILogger<AgricoltoreController> logger,
            BollettiniService bollettiniService,
            ColtureService coltureService,
            ProvinceService provinceService) : base(userService)
        {
            _logger = logger;
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

            var userWithPreferences = await _userService.Query(new GetUserWithPreferencesQuery { Id = user.Id });
            var query = new BollettiniListQuery
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

        public virtual IActionResult Bollettino()
        {
            return View();
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
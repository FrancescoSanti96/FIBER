using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Template.Services.Users;
using Template.Web.Areas.Dto;
using Template.Services.DataPersister;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;

namespace Template.Web.Areas.Agricoltore
{
    [Area("Agricoltore")]
    public class AgricoltoreController : AuthenticatedBaseController
    {
        private readonly IDataPersister _dataPersister;
        private readonly ILogger<AgricoltoreController> _logger;

        public AgricoltoreController(UserService userService, IDataPersister dataPersister, ILogger<AgricoltoreController> logger) : base(userService) 
        {
            _dataPersister = dataPersister;
            _logger = logger;
        }

        public virtual async Task<IActionResult> BollettiniAgricoltore()
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
            
            // Salva immediatamente nel JSON
            await _dataPersister.SaveOnFileAsync();
            
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

            // Salva immediatamente nel JSON
            await _dataPersister.SaveOnFileAsync();

            return RedirectToAction("ImpostazioniAgricoltore", "Agricoltore", new { area = "Agricoltore" });
        }
    }
}
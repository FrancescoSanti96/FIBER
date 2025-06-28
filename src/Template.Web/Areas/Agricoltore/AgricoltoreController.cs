using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Template.Services.Users;
using Template.Web.Areas.Dto;

namespace Template.Web.Areas.Agricoltore
{
    [Area("Agricoltore")]
    public class AgricoltoreController : AuthenticatedBaseController
    {

        public AgricoltoreController(UserService userService) : base(userService) { }

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

            // Carica le province della Romagna
            var provinces = await _userService.Query(new GetAllProvincesQuery());
            ViewBag.Provinces = provinces;
            
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

            // Carica tutte le colture
            var coltures = await _userService.Query(new GetAllColturesQuery());
            ViewBag.Coltures = coltures;
            
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
    }
}
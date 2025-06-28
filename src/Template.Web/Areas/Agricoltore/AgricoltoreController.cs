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
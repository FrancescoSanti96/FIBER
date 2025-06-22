using Template.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Template.Services.Shared;
using System.Threading.Tasks;
using Template.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Template.Web.Features.Login
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Alerts]
    [ModelStateToTempData]
    public partial class LoginController : Controller
    {
        public static string LoginErrorModelStateKey = "LoginError";
        private readonly SharedService _sharedService;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            SharedService sharedService, 
            IStringLocalizer<SharedResource> sharedLocalizer,
            ILogger<LoginController> logger)
        {
            _sharedService = sharedService;
            _sharedLocalizer = sharedLocalizer;
            _logger = logger;
        }

        private ActionResult LoginAndRedirect(UserDetailDTO utente, string returnUrl, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utente.Id.ToString()),
                new Claim(ClaimTypes.Email, utente.Email),
                new Claim(ClaimTypes.Role, utente.RoleId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
            {
                ExpiresUtc = (rememberMe) ? DateTimeOffset.UtcNow.AddMonths(3) : null,
                IsPersistent = rememberMe,
            });

            if (string.IsNullOrWhiteSpace(returnUrl) == false)
                return Redirect(returnUrl);

            // Reindirizza in base al ruolo
            switch (utente.RoleId)
            {
                case 1: // Admin
                    _logger.LogInformation("Admin login successful: {Email}", utente.Email);
                    Console.WriteLine($"Admin login successful: {utente.Email}");
                    return RedirectToAction("BollettiniAgricoltore", "Agricoltore", new { area = "Agricoltore" });
                case 2: // Agricoltore
                    return RedirectToAction("BollettiniAgricoltore", "Agricoltore", new { area = "Agricoltore" });
                case 3: // Tecnico
                    return RedirectToAction("BollettiniCaricati", "Tecnico", new { area = "Tecnico" });
            }

            // Se arriviamo qui, c'è un problema con il RoleId
            _logger.LogWarning("Login attempt with invalid RoleId: {RoleId}", utente.RoleId);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public virtual IActionResult Login(string returnUrl)
        {
            if (HttpContext.User != null && HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrWhiteSpace(returnUrl) == false)
                    return Redirect(returnUrl);

                // Reindirizza in base al ruolo dell'utente autenticato
                var roleClaim = HttpContext.User.FindFirst(ClaimTypes.Role);
                if (roleClaim != null && int.TryParse(roleClaim.Value, out int roleId))
                {
                    switch (roleId)
                    {
                        case 1: // Admin
                            _logger.LogInformation("Admin login successful: {Email}", HttpContext.User.FindFirst(ClaimTypes.Email)?.Value);
                            Console.WriteLine($"Admin login successful: {HttpContext.User.FindFirst(ClaimTypes.Email)?.Value}");
                            return RedirectToAction("BollettiniAgricoltore", "Agricoltore", new { area = "Agricoltore" });
                        case 2: // Agricoltore
                            return RedirectToAction("BollettiniAgricoltore", "Agricoltore", new { area = "Agricoltore" });
                        case 3: // Tecnico
                            return RedirectToAction("BollettiniCaricati", "Tecnico", new { area = "Tecnico" });
                    }
                }

                // Se arriviamo qui, c'è un problema con il RoleId
                _logger.LogWarning("Authenticated user with invalid RoleId");
                return RedirectToAction(nameof(Login));
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
            };

            return View(model);
        }

        [HttpPost]
        public async virtual Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var utente = await _sharedService.Query(new CheckLoginCredentialsQuery
                    {
                        Email = model.Email,
                        Password = model.Password,
                    });

                    return LoginAndRedirect(utente, model.ReturnUrl, model.RememberMe);
                }
                catch (LoginException e)
                {
                    ModelState.AddModelError(LoginErrorModelStateKey, e.Message);
                }
            }

            // Torna alla view con gli errori invece di fare redirect
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Logout()
        {
            HttpContext.SignOutAsync();

            Alerts.AddSuccess(this, "Utente scollegato correttamente");
            return RedirectToAction(nameof(Login));
        }
    }
}
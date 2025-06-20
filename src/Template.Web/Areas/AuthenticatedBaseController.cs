using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;
using Template.Web.Infrastructure;
using Newtonsoft.Json;

namespace Template.Web.Areas
{
    [Authorize]
    [Alerts]
    [ModelStateToTempData]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public partial class AuthenticatedBaseController : Controller
    {
        public AuthenticatedBaseController() { }

        protected IdentitaViewModel Identita
        {
            get
            {
                return (IdentitaViewModel)ViewData[IdentitaViewModel.VIEWDATA_IDENTITACORRENTE_KEY];
            }
        }

        protected int? CurrentUserRoleId
        {
            get
            {
                var roleClaim = HttpContext.User.FindFirst(ClaimTypes.Role);
                if (roleClaim != null && int.TryParse(roleClaim.Value, out int roleId))
                {
                    return roleId;
                }
                return null;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (context.HttpContext != null && context.HttpContext.User != null && context.HttpContext.User.Identity.IsAuthenticated)
                {
                    ViewData[IdentitaViewModel.VIEWDATA_IDENTITACORRENTE_KEY] = new IdentitaViewModel
                    {
                        EmailUtenteCorrente = context.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Email).First().Value
                    };

                    var area = context.RouteData.Values["area"]?.ToString();
                    if (!string.IsNullOrEmpty(area))
                    {
                        var roleId = CurrentUserRoleId;
                        if (roleId.HasValue)
                        {
                            bool hasAccess = area switch
                            {
                                "Agricoltore" => roleId == 1 || roleId == 2, // Admin e Agricoltore
                                "Tecnico" => roleId == 1 || roleId == 3,     // Admin e Tecnico
                                _ => true
                            };

                            if (!hasAccess)
                            {
                                // Reindirizza l'utente alla sua area in base al ruolo
                                var redirectUrl = roleId switch
                                {
                                    2 => "/Agricoltore/Agricoltore/BollettiniAgricoltore",  // Agricoltore
                                    3 => "/Tecnico/Tecnico/BollettiniCaricati",             // Tecnico
                                    _ => "/Login/Login"                                     // Altri casi (non dovrebbe mai succedere)
                                };

                                Alerts.AddError(this, "Non hai i permessi per accedere a questa area. Sei stato reindirizzato alla tua area di competenza.", 5000); // 5000ms = 5 secondi
                                // Salva manualmente gli alerts nel TempData prima del reindirizzamento
                                var controller = (Controller)context.Controller;
                                if (controller.ViewData.ContainsKey(Alerts.ALERTS_KEY))
                                {
                                    var viewDataAlerts = controller.ViewData[Alerts.ALERTS_KEY];
                                    if (viewDataAlerts != null)
                                    {
                                        controller.TempData[Alerts.ALERTS_KEY] = JsonConvert.SerializeObject(viewDataAlerts);
                                    }
                                }
                                context.Result = new RedirectResult(redirectUrl);
                                return;
                            }
                        }
                    }
                }

                base.OnActionExecuting(context);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected async Task<UserDetailDTO> GetCurrentUserAsync()
        {
            var userIdentity = HttpContext.User;
            var userId = userIdentity.FindFirst(ClaimTypes.NameIdentifier).ToString();

            if (int.TryParse(userId, out int intUserId))
            {
                var user = await _userService.Query(new UserDetailQuery { Id = intUserId });
                return user;
            }

            return null;
        }
    }
}

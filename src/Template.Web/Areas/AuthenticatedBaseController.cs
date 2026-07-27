using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;
using Template.Web.Infrastructure;
using Newtonsoft.Json;
using Template.Services.Users;
using System.Threading.Tasks;

namespace Template.Web.Areas
{
    [Authorize]
    [Alerts]
    [ModelStateToTempData]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AuthenticatedBaseController : Controller
    {
        protected readonly UserService _userService;

        public AuthenticatedBaseController(UserService userService)
        {
            _userService = userService;
        }

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
                    // Uso un approccio sincrono per evitare problemi con l'override asincrono
                    var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                    var firstName = "";
                    var lastName = "";
                    
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        var user = _userService.Query(new UserDetailQuery { Id = userId }).Result;
                        if (user != null)
                        {
                            firstName = user.FirstName ?? "";
                            lastName = user.LastName ?? "";
                        }
                    }

                    ViewData[IdentitaViewModel.VIEWDATA_IDENTITACORRENTE_KEY] = new IdentitaViewModel
                    {
                        EmailUtenteCorrente = context.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Email).First().Value,
                        FirstName = firstName,
                        LastName = lastName
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
                                    3 => "/Tecnico/Tecnico/HomeTecnico",                    // Tecnico
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
            catch
            {
                throw;
            }
        }

        protected async Task<UserDetailDTO> GetCurrentUserAsync()
        {
            var userIdentity = HttpContext.User;
            
            if (userIdentity == null || !userIdentity.Identity.IsAuthenticated)
            {
                return null;
            }

            var userIdClaim = userIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            if (userIdClaim == null)
            {
                return null;
            }
            
            if (int.TryParse(userIdClaim.Value, out int intUserId))
            {
                var user = await _userService.Query(new UserDetailQuery { Id = intUserId });
                return user;
            }

            return null;
        }
    }
}

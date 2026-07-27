using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Template.EntityModel.Models;
using Template.Services.Bulletins;
using Template.Services.Coltures;
using Template.Services.Provinces;
using Template.Services.Users;
using Template.Web.Areas.Tecnico.Enums;
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
        private readonly BollettiniService _bollettiniService;

        public TecnicoController(UserService userService,
            ColtureService coltureService,
            ProvinceService provinceService,
            IPdfService pdfService,
            BollettiniService bollettiniService)
            : base(userService)
        {
            _coltureService = coltureService;
            _provinceService = provinceService;
            _pdfService = pdfService;
            _bollettiniService = bollettiniService;
        }

        // GET: Tecnico/Tecnico/HomeTecnico
        public async Task<IActionResult> HomeTecnico(HomeTecnicoViewModel vm)
        {
            try
            {
                vm.User = await GetCurrentUserAsync();
                vm.Colture = [.. (await _coltureService.Query()).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),

                })];

                var queryDto = new BulletinListQuery
                {
                    Paging = new Template.Infrastructure.Paging
                    {
                        OrderBy = vm.OrderBy,
                        OrderByDescending = vm.OrderByDescending,
                        Page = vm.Page,
                        PageSize = vm.PageSize
                    },
                    FilterExpression = vm.Tab == TabBollettini.Caricati
                        ? x => x.Published == true
                        : x => x.Published == false,
                    ColtureFilter = vm.IdColtureSelezionate
                };

                var bulletins = await _bollettiniService.Query(queryDto);
                vm.SetBollettini(bulletins);

                return View(vm);
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, ex.Message);
                return View(vm);
            }
        }

        // GET: Tecnico/Tecnico/BollettinoTecnico
        public virtual async Task<IActionResult> BollettinoTecnico(int id, bool isDraft)
        {
            try
            {
                var bulletinDto = await _bollettiniService.Query(new GetBulletinByIdQuery { Id = id });
                
                if (bulletinDto == null)
                {
                    Alerts.AddError(this, "Bollettino non trovato");
                    // Se è una bozza, torna alle bozze, altrimenti ai caricati
                    var tab = isDraft || bulletinDto?.Published == false ? TabBollettini.Bozze : TabBollettini.Caricati;
                    return RedirectToAction(nameof(HomeTecnico), new { Tab = tab });
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
                    Province = bulletinDto.Provinces,
                    Colture = bulletinDto.Coltures,
                    Pubblicato = bulletinDto.Published,
                    IsDraft = !bulletinDto.Published
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, $"Errore nel caricamento del bollettino: {ex.Message}");
                // Se è una bozza, torna alle bozze, altrimenti ai caricati
                var tab = isDraft ? TabBollettini.Bozze : TabBollettini.Caricati;
                return RedirectToAction(nameof(HomeTecnico), new { Tab = tab });
            }
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

                    var newBulletinId = await _bollettiniService.AddNewBulletinAsync(bulletin, provinceIds: model.IdProvinceSelezionate, coltureIds: model.IdColtureSelezionate);
                    
                    // Aggiungi notifica di successo basata sullo stato del bollettino
                    if (model.Pubblicato)
                    {
                        Alerts.AddSuccess(this, "Bollettino pubblicato con successo! È ora visibile agli agricoltori.", 4000);
                    }
                    else
                    {
                        Alerts.AddSuccess(this, "Bollettino salvato come bozza. Potrai modificarlo e pubblicarlo in seguito.", 4000);
                    }

                    return model.TornaAiBollettini
                        ? RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Bozze })
                        : RedirectToAction(nameof(BollettinoTecnico), new { Id = newBulletinId });
                }
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, ex.Message);
                return View(model);
            }
        }

        // GET: Tecnico/Tecnico/ModificaBollettino
        public async Task<IActionResult> ModificaBollettino(int id)
        {
            try
            {
                var vm = await GetModificaBollettinoViewModel();
                var bulletinDto = await _bollettiniService.Query(new GetBulletinByIdQuery { Id = id });
                
                if (bulletinDto == null)
                {
                    Alerts.AddError(this, "Bollettino non trovato");
                    return RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Bozze });
                }

                vm.Id = bulletinDto.Id;
                vm.IdColtureSelezionate = [.. bulletinDto.Coltures.Select(c => int.Parse(vm.OpzioniColture.FirstOrDefault(x => x.Text == c).Value))];
                vm.IdProvinceSelezionate = [.. bulletinDto.Provinces.Select(p => int.Parse(vm.OpzioniProvince.FirstOrDefault(x => x.Text == p).Value))];
                vm.ContenutoBollettino = bulletinDto.Body;
                vm.TitoloBollettino = bulletinDto.Summary;
                vm.Scadenza = bulletinDto.ExpireDate;
                vm.Pubblicato = bulletinDto.Published;

                return View(vm);
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, $"Errore nel caricamento del bollettino: {ex.Message}");
                return RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Bozze });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ModificaBollettino(ModificaBollettinoViewModel model)
        {
            var baseVm = await GetModificaBollettinoViewModel();
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

                    var updatedBulletinId = await _bollettiniService.UpdateBulletinAsync(new UpdateBulletinDto
                    {
                        Id = model.Id,
                        IdUser = currentUser.Id,
                        Body = model.ContenutoBollettino,
                        Published = model.Pubblicato,
                        ColtureIds = model.IdColtureSelezionate,
                        ProvinceIds = model.IdProvinceSelezionate,
                        ExpireDate = model.Scadenza,
                        Summary = model.TitoloBollettino
                    });

                    // Aggiungi notifica di successo per la modifica
                    if (model.Pubblicato)
                    {
                        Alerts.AddSuccess(this, "Bollettino modificato e pubblicato con successo! Le modifiche sono ora visibili agli agricoltori.", 5000);
                    }
                    else
                    {
                        Alerts.AddSuccess(this, "Bollettino modificato e salvato come bozza. Ricorda di pubblicarlo quando pronto.", 4000);
                    }

                    return model.TornaAiBollettini
                        ? RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Bozze })
                        : RedirectToAction(nameof(BollettinoTecnico), new { Id = updatedBulletinId });
                }
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, ex.Message);
                return View(model);
            }
        }

        // GET: Tecnico/Tecnico/DownloadBollettino
        public async Task<IActionResult> DownloadBollettino(int id)
        {
            try
            {
                var bulletinDto = await _bollettiniService.Query(new GetBulletinByIdQuery { Id = id });
                
                if (bulletinDto == null)
                {
                    Alerts.AddError(this, "Bollettino non trovato");
                    // Se il bollettino non esiste, torna ai caricati (dove normalmente dovrebbe essere)
                    return RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Caricati });
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
                return RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Caricati });
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
        private async Task<ModificaBollettinoViewModel> GetModificaBollettinoViewModel()
        {
            var vm = await GetNuovoBollettinoViewModel();
            return new ModificaBollettinoViewModel
            {
                OpzioniColture = vm.OpzioniColture,
                OpzioniProvince = vm.OpzioniProvince,
            };
        }
        #endregion
    }
}
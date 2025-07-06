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

                var queryDto = new BollettiniListQuery
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

                var bollettini = (await _bollettiniService.Query(queryDto)).Select(x => new BollettinoCardViewModel(x)).ToList();

                vm.Bollettini = bollettini;
                vm.TotalItems = bollettini.Count;

                return View(vm);
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, ex.Message);
                return View(vm);
            }
        }

        // GET: Tecnico/Tecnico/BollettinoTecnico
        public virtual async Task<IActionResult> BollettinoTecnico(int id = 3, bool isDraft = false)
        {
            try
            {
                var bulletinDto = await _userService.Query(new GetBulletinByIdQuery { Id = id });
                
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
                    Province = bulletinDto.ProvinceNames,
                    Colture = bulletinDto.ColtureNames,
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

                    var newBulletinId = await _userService.AddNewBulletinAsync(bulletin, provinceIds: model.IdProvinceSelezionate, coltureIds: model.IdColtureSelezionate);
                    return RedirectToAction(nameof(BollettinoTecnico), new { Id = newBulletinId });
                }
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, ex.Message);
                return View(model);
            }
        }

        // GET: Tecnico/Tecnico/ModificaBollettino
        public virtual async Task<IActionResult> ModificaBollettino(int id)
        {
            try
            {
                var bulletinDto = await _userService.Query(new GetBulletinByIdQuery { Id = id });
                
                if (bulletinDto == null)
                {
                    Alerts.AddError(this, "Bollettino non trovato");
                    return RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Bozze });
                }

                // Verifica che sia una bozza (non pubblicato)
                if (bulletinDto.Published)
                {
                    Alerts.AddError(this, "Non è possibile modificare un bollettino già pubblicato");
                    return RedirectToAction(nameof(BollettinoTecnico), new { id, isDraft = false });
                }

                var model = new ModificaBollettinoViewModel
                {
                    Id = bulletinDto.Id,
                    ContenutoBollettino = bulletinDto.Body ?? "",
                    TitoloBollettino = bulletinDto.Summary ?? "",
                    Scadenza = bulletinDto.ExpireDate,
                    Pubblicato = bulletinDto.Published,
                    IdColtureSelezionate = bulletinDto.ColtureIds,
                    IdProvinceSelezionate = bulletinDto.ProvinceIds,
                    OpzioniColture = [.. (await _coltureService.Query()).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                    })],
                    OpzioniProvince = [.. (await _provinceService.Query()).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                    })],
                    
                    // Proprietà legacy per compatibilità
                    Title = bulletinDto.Summary ?? "Bollettino senza titolo",
                    Content = bulletinDto.Body ?? "",
                    NomeBollettino = bulletinDto.Summary ?? "Bollettino senza titolo",
                    CulturaInteresse = string.Join(", ", bulletinDto.ColtureNames ?? new List<string>()),
                    ZonaInteresse = string.Join(", ", bulletinDto.ProvinceNames ?? new List<string>()),
                    ScadenzaTemporale = bulletinDto.ExpireDate?.ToString("dd/MM/yyyy") ?? ""
                };

                return View(model);
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
            // Popola le opzioni per il dropdown in caso di errore
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

                    // Recupera il bollettino esistente
                    var existingBulletin = await _userService.Query(new GetBulletinByIdQuery { Id = model.Id });
                    if (existingBulletin == null)
                    {
                        Alerts.AddError(this, "Bollettino non trovato");
                        return RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Bozze });
                    }

                    // Verifica che sia una bozza
                    if (existingBulletin.Published)
                    {
                        Alerts.AddError(this, "Non è possibile modificare un bollettino già pubblicato");
                        return RedirectToAction(nameof(BollettinoTecnico), new { id = model.Id, isDraft = false });
                    }

                    // Aggiorna il bollettino
                    var updatedBulletin = new Bulletin
                    {
                        Id = model.Id,
                        IdUser = currentUser.Id,
                        Summary = model.TitoloBollettino,
                        Body = model.ContenutoBollettino,
                        Published = model.Pubblicato,
                        ExpireDate = model.Scadenza
                    };

                    await _userService.UpdateBulletinAsync(updatedBulletin, provinceIds: model.IdProvinceSelezionate, coltureIds: model.IdColtureSelezionate);
                    
                    Alerts.AddSuccess(this, "Bollettino modificato con successo!");
                    
                    // Reindirizza alla vista del bollettino o alla lista appropriata
                    if (model.Pubblicato)
                    {
                        return RedirectToAction(nameof(BollettinoTecnico), new { Id = model.Id, isDraft = false });
                    }
                    else
                    {
                        return RedirectToAction(nameof(HomeTecnico), new { Tab = TabBollettini.Bozze });
                    }
                }
            }
            catch (Exception ex)
            {
                Alerts.AddError(this, ex.Message);
                return View(model);
            }
        }

        // GET: Tecnico/Tecnico/DownloadBollettino
        public virtual async Task<IActionResult> DownloadBollettino(int id)
        {
            try
            {
                var bulletinDto = await _userService.Query(new GetBulletinByIdQuery { Id = id });
                
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
                var provinces = bulletinDto.ProvinceNames?.ToList() ?? new List<string>();
                var coltures = bulletinDto.ColtureNames?.ToList() ?? new List<string>();

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
        #endregion
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using Template.EntityModel.Models;
using Template.Services.Bulletins;
using Template.Services.Users;
using Template.Web.Areas.Tecnico.Enums;
using Template.Web.Infrastructure;
using Template.Web.Models;

namespace Template.Web.Areas.Tecnico.ViewModels
{
    public class HomeTecnicoViewModel : PagingViewModel
    {
        public override string ActionName => nameof(TecnicoController.HomeTecnico);
        public override string ControllerName => "Tecnico";
        public UserDetailDTO User { get; set; }
        public TabBollettini Tab { get; set; } = TabBollettini.Bozze;
        public List<BollettinoCardViewModel> Bollettini { get; set; } = [];
        public List<SelectListItem> Colture { get; set; } = [];
        public List<int> IdColtureSelezionate { get; set; } = [];


        public HomeTecnicoViewModel()
        {
            OrderBy = nameof(Bulletin.ExpireDate);
            OrderByDescending = true;
        }

        public override RouteValueDictionary GetRouteValues()
        {
            return new RouteValueDictionary
            {
                { "Tab", Tab },
                { "Page", Page },
                { "PageSize", PageSize },
                { "OrderBy", OrderBy },
                { "OrderByDescending", OrderByDescending }
            };
        }

    }
}

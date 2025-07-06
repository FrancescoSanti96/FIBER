using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using Template.EntityModel.Models;
using Template.Web.Infrastructure;
using Template.Web.Models;

namespace Template.Web.Areas.Agricoltore.ViewModels
{
    public class BollettiniAgricoltoreViewModel : PagingViewModel
    {
        public override string ActionName => nameof(AgricoltoreController.BollettiniAgricoltore);
        public override string ControllerName => "Agricoltore";
        public List<BollettinoCardViewModel> Bollettini { get; set; } = [];
        public BollettiniAgricoltoreViewModel()
        {
            OrderBy = nameof(Bulletin.ExpireDate);
            OrderByDescending = true;
        }

        public override RouteValueDictionary GetRouteValues()
        {
            return new RouteValueDictionary
            {
                { "Page", Page },
                { "PageSize", PageSize },
                { "OrderBy", OrderBy },
                { "OrderByDescending", OrderByDescending }
            };
        }
    }
}

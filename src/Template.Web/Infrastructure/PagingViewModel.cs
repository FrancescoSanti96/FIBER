using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

namespace Template.Web.Infrastructure
{
    public abstract class PagingViewModel
    {
        public abstract string ControllerName { get; }
        public abstract string ActionName { get; }

        public int Page { get; set; }
        [Display(Name = "Elementi per pagina")]
        public int PageSize { get; set; }
        [Display(Name = "Elementi")]
        public int TotalItems { get; set; }
        public string OrderBy { get; set; }
        public bool OrderByDescending { get; set; }
        public int[] PageSizes { get; set; }

        public IEnumerable<SelectListItem> PageSizeListItems // VB: non facciamo propriet? che fanno i conti, meglio un metodo
        {
            get
            {
                return PageSizes.Select(x => new SelectListItem
                {
                    Text = x.ToString(),
                    Value = x.ToString()
                });
            }
        }

        public PagingViewModel()
        {
            Page = 1;
            SetPagingDefaults();
        }

        public virtual void SetPagingDefaults()
        {
            PageSize = 25;
            PageSizes = new int[] { 15, 25, 50, 100 };
        }

        public int TotalPages()
        {
            return (int)Math.Max(1, Math.Ceiling((double)TotalItems / PageSize));
        }

        public abstract RouteValueDictionary GetRouteValues();


        public string ChangePageSizePageUrl(IUrlHelper url, int pageSize)
        {
            var routeValues = GetRouteValues();
            return ChangePageSizePageUrl(url, routeValues, pageSize);
        }
        string ChangePageSizePageUrl(IUrlHelper url, RouteValueDictionary routeValues, int pageSize)
        {
            routeValues["PageSize"] = pageSize;
            return url.Action(action: ActionName, controller: ControllerName, values: routeValues);
        }


        public string NextPageUrl(IUrlHelper url)
        {
            var routeValues = GetRouteValues();
            return NextPageUrl(url, routeValues);
        }
        string NextPageUrl(IUrlHelper url, RouteValueDictionary routeValues)
        {
            routeValues["Page"] = Math.Min(TotalPages(), Page + 1);
            return url.Action(action: ActionName, controller: ControllerName, values: routeValues);
        }
        //string NextPageUrl(IUrlHelper url, Task<ActionResult> route)
        //{
        //    return NextPageUrl(url, route.GetAwaiter().GetResult());

        //}

        public string LastPageUrl(IUrlHelper url)
        {
            var routeValues = GetRouteValues();
            return LastPageUrl(url, routeValues);
        }

        string LastPageUrl(IUrlHelper url, RouteValueDictionary routeValues)
        {
            routeValues["Page"] = TotalPages();
            return url.Action(action: ActionName, controller: ControllerName, values: routeValues);
        }
        public string PrevPageUrl(IUrlHelper url)
        {
            var routeValues = GetRouteValues();
            return PrevPageUrl(url, routeValues);
        }
        //string PrevPageUrl(IUrlHelper url, Task<ActionResult> route)
        //{
        //    return PrevPageUrl(url, route.GetAwaiter().GetResult());
        //}
        string PrevPageUrl(IUrlHelper url, RouteValueDictionary routeValues)
        {
            routeValues["Page"] = Math.Max(1, Page - 1);
            return url.Action(action: ActionName, controller: ControllerName, values: routeValues);
        }
        public string FirstPageUrl(IUrlHelper url)
        {
            var routeValues = GetRouteValues();
            return FirstPageUrl(url, routeValues);
        }

        string FirstPageUrl(IUrlHelper url, RouteValueDictionary routeValues)
        {
            routeValues["Page"] = 1;
            return url.Action(action: ActionName, controller: ControllerName, values: routeValues);
        }


        protected string OrderbyUrl<TModel, TProperty>(IUrlHelper url, Expression<Func<TModel, TProperty>> expression)
        {
            var propertyName = GetModelExpressionProvider(url.ActionContext.HttpContext.RequestServices).GetExpressionText(expression);
            return OrderbyUrl(url, propertyName, GetRouteValues());
        }

        public string OrderbyUrl(IUrlHelper url, string propertyName)
        {
            return OrderbyUrl(url, propertyName, GetRouteValues());
        }

        string OrderbyUrl(IUrlHelper url, string propertyName, RouteValueDictionary routeValues)
        {
            if (OrderBy == propertyName)
            {
                routeValues["OrderByDescending"] = !OrderByDescending;
            }
            else
            {
                routeValues["OrderBy"] = propertyName;
                routeValues["OrderByDescending"] = false;
            }

            return url.Action(action: ActionName, controller: ControllerName, values: routeValues);
        }

        protected string OrderbyCss<TModel, TProperty>(HttpContext context, Expression<Func<TModel, TProperty>> expression)
        {
            var propertyName = GetModelExpressionProvider(context.RequestServices).GetExpressionText(expression);
            return OrderbyCss(propertyName);
        }

        public string OrderbyCss(string propertyName)
        {
            var na = "fa-solid fa-sort";
            var down = "fa-solid fa-sort-down";
            var up = "fa-solid fa-sort-up";
            if (OrderBy == propertyName)
            {
                if (OrderByDescending)
                    return down;
                else
                    return up;
            }
            else
                return na;
        }

        Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpressionProvider GetModelExpressionProvider(IServiceProvider services)
        {
            return services.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpressionProvider)) as Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpressionProvider;
        }
    }
}
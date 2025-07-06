using Template.Services.Users;
using Microsoft.Extensions.DependencyInjection;
using Template.Services.Coltures;
using Template.Services.Provinces;
using Template.Web.Services;

namespace Template.Web
{
    public class Container
    {
        public static void RegisterTypes(IServiceCollection container)
        {
            // Registration of all the database services you have
            container.AddScoped<UserService>();
            container.AddScoped<ColtureService>();
            container.AddScoped<ProvinceService>();
            
            // PDF Service (QuestPDF)
            container.AddScoped<IPdfService, PdfService>();
        }
    }
}

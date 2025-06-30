using Template.Services.Users;
using Microsoft.Extensions.DependencyInjection;
using Template.Services.DataPersister;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Template.Infrastructure;
using Template.Services.Coltures;
using Template.Services.Provinces;

namespace Template.Web
{
    public class Container
    {
        public static void RegisterTypes(IServiceCollection container)
        {
            // Registration of all the database services you have
            container.AddScoped<IDataPersister, DataPersister>();
            container.AddSingleton<IInterceptor, FiberDbContextSaveChangesInterceptor>();

            // Repository
            container.AddScoped<UserService>();
            container.AddScoped<ColtureService>();
            container.AddScoped<ProvinceService>();
        }
    }
}

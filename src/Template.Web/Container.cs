using Template.Services.Users;
using Microsoft.Extensions.DependencyInjection;
using Template.Services.DataPersister;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Template.Infrastructure;

namespace Template.Web
{
    public class Container
    {
        public static void RegisterTypes(IServiceCollection container)
        {
            // Registration of all the database services you have
            container.AddScoped<IDataPersister, DataPersister>();
            container.AddSingleton<IInterceptor, FiberDbContextSaveChangesInteceptor>();
            container.AddScoped<UserService>();
        }
    }
}

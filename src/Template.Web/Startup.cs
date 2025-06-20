//using Template.Web.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using Template.EntityModel;
using Template.Services.DataPersister;
using Template.Web.Infrastructure;

namespace Template.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Env = env;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddDbContext<FiberDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<IInterceptor>();
                options.UseInMemoryDatabase(databaseName: "Fiber")
                    .AddInterceptors(interceptor);
            });

            // SERVICES FOR AUTHENTICATION
            services.AddSession();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Login/Login";
                options.LogoutPath = "/Login/Logout";
            });

            var builder = services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {                        // Enable loading SharedResource for ModelLocalizer
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

#if DEBUG
            builder.AddRazorRuntimeCompilation();
#endif

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.AreaViewLocationFormats.Clear();
                options.AreaViewLocationFormats.Add("/Areas/{2}/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");

                options.ViewLocationFormats.Clear();
                options.ViewLocationFormats.Add("/Features/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Features/Views/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Features/Views/Shared/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            });

            // SIGNALR FOR COLLABORATIVE PAGES
            services.AddSignalR();

            // CONTAINER FOR ALL EXTRA CUSTOM SERVICES
            Container.RegisterTypes(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, IServiceScopeFactory scopeFactory)
        {
            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                // Https redirection only in production
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Localization support if you want to
            app.UseRequestLocalization(SupportedCultures.CultureNames);

            app.UseRouting();

            // Adding authentication to pipeline
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            var node_modules = new CompositePhysicalFileProvider(Directory.GetCurrentDirectory(), "node_modules");
            var areas = new CompositePhysicalFileProvider(Directory.GetCurrentDirectory(), "Areas");
            var compositeFp = new CustomCompositeFileProvider(env.WebRootFileProvider, node_modules, areas);
            env.WebRootFileProvider = compositeFp;
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute("Agricoltore", "Agricoltore", "Agricoltore/{controller=Agricoltore}/{action=BollettiniAgricoltore}/{id?}");
                endpoints.MapAreaControllerRoute("Tecnico", "Tecnico", "Tecnico/{controller=Tecnico}/{action=HomeTecnico}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Login}/{action=Login}");
            });

            // Registrazione del salvataggio automatico quando l'app si arresta
            lifetime.ApplicationStopped.Register(() =>
            {
                using var scope = scopeFactory.CreateScope();
                var persister = scope.ServiceProvider.GetRequiredService<IDataPersister>();
                persister.SaveOnFileAsync().GetAwaiter().GetResult();
            });
        }
    }

    public static class SupportedCultures
    {
        public readonly static string[] CultureNames;
        public readonly static CultureInfo[] Cultures;

        static SupportedCultures()
        {
            CultureNames = new[] { "it-it" };
            Cultures = CultureNames.Select(c => new CultureInfo(c)).ToArray();

            //NB: attenzione nel progetto a settare correttamente <NeutralLanguage>it-IT</NeutralLanguage>
        }
    }
}

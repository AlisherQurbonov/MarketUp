using MarketUp.Components;
using MarketUp.Helpers;
using MarketUp.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using System.Globalization;

namespace MarketUp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                if (NetworkHelper.IsLocale())
                {
                    HttpClient.DefaultProxy = NetworkHelper.GetDefaultProxy();
                }

                IdentityModelEventSource.ShowPII = true;
            }

            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }
       
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new AppConfig();
            Configuration.GetSection("Config").Bind(config);
            services.AddSingleton(config);

            services.ConfigureNonBreakingSameSiteCookies();

            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
            services.AddRazorPages();
            services.AddServerSideBlazor();
                                
            services.AddHttpContextAccessor();

            services.AddRazorComponents()
           .AddInteractiveServerComponents();          

            services.AddTransient<HandbookService>();         

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
            });

            services.AddRazorPages()
                .AddNewtonsoftJson();
        
            services.Configure<RequestLocalizationOptions>(a =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("ru"),
                    new CultureInfo("uz"),
                    new CultureInfo("uz-Latn-UZ")
                };

                a.DefaultRequestCulture = new RequestCulture("uz");
                a.SupportedCultures = supportedCultures;
                a.SupportedUICultures = supportedCultures;
                a.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                };
            });

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Resources"; }).AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    })
                .AddDataAnnotationsLocalization();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => builder.SetIsOriginAllowed(a => true)
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader());
            });
         
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddDirectoryBrowser();
        }

        public void Configure(IApplicationBuilder app)
        {

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseCors("AllowAll");


            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);


            if (Environment.IsProduction())
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                app.Use(async (req, next) =>
                {
                    req.Request.Scheme = "https";

                    await next();
                });
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
          

            app.UseHttpsRedirection();
            app.UseAntiforgery();

            app.UseEndpoints(endpoints =>
            {              
                endpoints.MapRazorComponents<App>().AddInteractiveServerRenderMode();              
            });
         
        }
    }
}

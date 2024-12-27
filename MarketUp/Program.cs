using MarketUp.Components;
using Serilog;

namespace MarketUp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseSerilog((hst, cnf) =>
                {
                    cnf.ReadFrom.Configuration(hst.Configuration);
                    cnf.Enrich.FromLogContext();
                    cnf.Enrich.WithProperty("ApplicationName", hst.HostingEnvironment.ApplicationName);
                    cnf.WriteTo.Console().MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning);
                    cnf.WriteTo.Debug();
                    cnf.WriteTo.File("Logs/web.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true);
                });

                webBuilder.UseStartup<Startup>();
            });
    }
}


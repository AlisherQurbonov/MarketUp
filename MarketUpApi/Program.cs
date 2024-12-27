using Serilog;

namespace MarketUpApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Market Api";

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
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

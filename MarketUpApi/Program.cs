using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

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
                        cnf.WriteTo.GrafanaLoki("http://localhost:3100");                      
                    });

                    webBuilder.UseMetrics(options =>
                    {
                        var metrics = AppMetrics.CreateDefaultBuilder()
                           .OutputMetrics.AsPrometheusPlainText()
                            .Build();
                        options.EndpointOptions = endpointsOptions =>
                        {
                            endpointsOptions.MetricsEndpointOutputFormatter =
                                metrics.OutputMetricsFormatters
                                       .OfType<MetricsPrometheusTextOutputFormatter>()
                                       .First();

                            endpointsOptions.MetricsTextEndpointEnabled = true;
                        };
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}

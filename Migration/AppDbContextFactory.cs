using MarketUpApi.Data;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Migration
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        private readonly string _connectionString;
        private const string NamespaceName = "MarketUp.Migrator";

        public AppDbContextFactory()
        {
            var builder = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", false, true)
           .AddEnvironmentVariables();
            var configuration = builder.Build();

            _connectionString = configuration["ConnectionStrings:DefaultConnection"]; ;
        }

        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseNpgsql(_connectionString, b => b.MigrationsAssembly(NamespaceName));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

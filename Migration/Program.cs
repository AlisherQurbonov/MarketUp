using Microsoft.Extensions.Configuration;

namespace Migration
{
    public static class Program
    {     
        private static async Task MigrateDb(IConfigurationRoot configuration)
        {
            var connectionString = configuration["ConnectionStrings:DefaultConnection"];
            var factory = new AppDbContextFactory(connectionString);

            await using var db = factory.CreateDbContext(null);
            if (await db.Database.CanConnectAsync())
                await db.Database.EnsureCreatedAsync();
                Console.WriteLine("DB:ok ...");

        }
    }
}

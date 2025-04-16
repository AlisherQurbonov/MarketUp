using Microsoft.Extensions.Configuration;

namespace Migration
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Title = "MarketUp Migrator";

            var factory = new AppDbContextFactory();

            await using var db = factory.CreateDbContext(args);
           
            Console.WriteLine("Database:ok ...");
         
            Console.WriteLine("Done!");
        }
    }
}

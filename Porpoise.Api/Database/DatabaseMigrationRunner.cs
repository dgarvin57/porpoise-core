using DbUp;
using System.Reflection;

namespace Porpoise.Api.Database
{
    /// <summary>
    /// Handles automatic database migrations using DbUp.
    /// Runs SQL scripts from Porpoise.DataAccess/Scripts/Migrations on application startup.
    /// </summary>
    public static class DatabaseMigrationRunner
    {
        public static void RunMigrations(string connectionString)
        {
            Console.WriteLine("🔄 Checking for database migrations...");
            
            // Get the path to the migrations folder
            var migrationsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", 
                "Porpoise.DataAccess", "Scripts", "Migrations");
            
            // Normalize the path
            migrationsPath = Path.GetFullPath(migrationsPath);
            
            if (!Directory.Exists(migrationsPath))
            {
                Console.WriteLine($"⚠️  Migrations folder not found at: {migrationsPath}");
                Console.WriteLine("   Skipping migrations.");
                return;
            }
            
            Console.WriteLine($"📁 Migrations folder: {migrationsPath}");
            
            // Configure DbUp to run scripts from the file system
            var upgrader = DeployChanges.To
                .MySqlDatabase(connectionString)
                .WithScriptsFromFileSystem(migrationsPath)
                .WithTransactionPerScript()
                .LogToConsole()
                .Build();
            
            var result = upgrader.PerformUpgrade();
            
            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Migration failed: {result.Error}");
                Console.ResetColor();
                throw new Exception("Database migration failed", result.Error);
            }
            
            if (result.Scripts.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Successfully ran {result.Scripts.Count()} migration(s)");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("✅ Database is up to date - no migrations needed");
            }
        }
    }
}

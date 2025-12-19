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
            
            // Load migrations from embedded resources in Porpoise.DataAccess assembly
            var dataAccessAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Porpoise.DataAccess");
            
            if (dataAccessAssembly == null)
            {
                Console.WriteLine("⚠️  Porpoise.DataAccess assembly not found");
                Console.WriteLine("   Skipping migrations.");
                return;
            }
            
            Console.WriteLine($"📦 Loading migrations from embedded resources...");
            
            // Configure DbUp to run scripts from embedded resources (MySQL-specific only)
            var upgrader = DeployChanges.To
                .MySqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(dataAccessAssembly, script => 
                    script.EndsWith("_MySQL.sql", StringComparison.OrdinalIgnoreCase))
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

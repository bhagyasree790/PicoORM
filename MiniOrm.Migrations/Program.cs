using MiniOrm.Migration.Commands;
using MiniOrm.Data;
using MiniOrm.Models;

//var connectionString = Environment.GetEnvironmentVariable("MINIORM_CONN");
var connectionString = "Host=localhost;Port=5432;Database=miniorm;Username=postgres;Password=0987654321";
if(string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Please set the MINIORM_CONN environment variable with your database connection string.");
    return;
}

using var db = new AppDbContext(connectionString);

if(args.Length<2 || args[0] != "migrations") //It checks if I actually typed migrations and provided a command like add. It will safe from program crashing.
{
    Console.WriteLine("Usage: dotnet run -- migrations <command> [options]");
    Console.WriteLine("Commands:");
    Console.WriteLine("add <After write add give a name to migration>");
    Console.WriteLine("apply");
    return;
}

var command = args[1].ToLower();

switch(command)
{
    case "add":
        if(args.Length < 3) 
        {
            Console.WriteLine("Migration Name:\ndotnet run -- migrations add <Name>");
            return;
        }
        var name = args[2]; //checking for the migration name provided or not.
        
        var entities = new List<Type> { typeof(Product) }; //Here, I'm saying look at the product class and prepare a table for it.
        MigrationRunner.GenerateMigration(name, entities); //It will trigger my TypeMapper reflection logic and converts  that C# info intoa CREATE TABLE string and save it as a .sql file.
        break;

    case "apply":
        Console.WriteLine("Applying migrations...");  //read the .sql file and execute by using Npgsql.
        MigrationRunner.ApplyMigrations(connectionString);
        break;

    case "list":
        Console.WriteLine("Listing applied migrations...");
        MigrationRunner.ListMigrations(connectionString);
        break;

    case "rollback":
        Console.WriteLine("Rolling back the last migration...");
        MigrationRunner.RollbackLastMigration(connectionString);
        break;

    default:
        Console.WriteLine($"Unknown command: {command}");
        break;
}

using System.Text;
using MiniOrm.Data;
using Npgsql;

namespace MiniOrm.Migration.Commands;

public static class MigrationRunner
{
    public static void GenerateMigration(string name, IEnumerable<Type> entityTypes)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var fileNAME = $"{timestamp}_{name}.sql";
        var upScript = new StringBuilder();
        var downScript = new StringBuilder();

        upScript.AppendLine("-- up");
        downScript.AppendLine("-- down");

        foreach(var type in entityTypes)
        {
            var metadata = TypeMapper.GetEntityMetadata(type);

            upScript.AppendLine($"CREATE TABLE IF NOT EXISTS {metadata.TableName} (");  //By using metadata It will create table in the up section.

            var columnDefs = metadata.Columns.Select(c => $" {c.ColumnName} {c.PostgresTYpe}");
            
            upScript.AppendLine(string.Join(",\n", columnDefs));
            upScript.AppendLine(");");


            downScript.AppendLine($"DROP TABLE IF EXISTS {metadata.TableName};");  //In the down section, it will drop the table if exists. It is for rollback purpose. If I want to undo the migration, I can run the down script and it will remove the table from database.
        }


        var fullContent = $"{upScript}\n{downScript}";
        Directory.CreateDirectory("Migrations");
        File.WriteAllText(Path.Combine("Migrations", fileNAME), fullContent);

        Console.WriteLine($"Created migration: {fileNAME}");
    }

    public static void ApplyMigrations(string connectionString)
    {
       using var connection = new Npgsql.NpgsqlConnection(connectionString);
       
       connection.Open();

       using(var cmd = new Npgsql.NpgsqlCommand("CREATE TABLE IF NOT EXISTS migrations (Id SERIAL PRIMARY KEY, Name TEXT, AppliedOn TIMESTAMP);", connection)) //to track the migration.
       {
            cmd.ExecuteNonQuery();
       }

       var files = Directory.GetFiles("Migrations", "*.sql").OrderBy(f => f).ToList();

       foreach(var file in files)
        {
            var fileName = Path.GetFileName(file);

            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM migrations WHERE name = @name", connection))
            {
                cmd.Parameters.AddWithValue("@name", fileName);
                var count = (long)cmd.ExecuteScalar();
                if(count > 0)
                {
                    continue; //It's mean the migration is already applied and skip it.
                }

            }

            Console.WriteLine("Applying migration: " + fileName);

            //Now I have to extract the up section.
            var content = File.ReadAllText(file);
            int start = content.IndexOf("-- up") + 5; 
            int end = content.IndexOf("-- down");
            var upSql = content.Substring(start, end - start).Trim();  // --up er next line thheke shuru kore --down er agey porjontho.

            using var transaction = connection.BeginTransaction();
            try
            {
                using var cmd = new NpgsqlCommand(upSql, connection, transaction);
                cmd.ExecuteNonQuery();

                using var insertCmd = new NpgsqlCommand("INSERT INTO migrations (Name, AppliedOn) VALUES (@name, @appliedOn)", connection, transaction);
                insertCmd.Parameters.AddWithValue("@name", fileName);
                insertCmd.Parameters.AddWithValue("@appliedOn", DateTime.Now);
                insertCmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to apply migration {fileName}: {ex.Message}");
                transaction.Rollback();
            }
        }
    }

    public static void ListMigrations(string connectionStrings)
    {
        using var connection = new NpgsqlConnection(connectionStrings);
        connection.Open();

        var appliedMigrations = new HashSet<string>();
        using (var cmd = new NpgsqlCommand("SELECT name FROM migrations", connection))
        {
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                appliedMigrations.Add(reader.GetString(0));
            }
        }
        Console.WriteLine("Applied Migrations:");
        foreach (var migration in appliedMigrations)
        {
            Console.WriteLine($" - {migration}");
        }
    }


    public static void RollbackLastMigration(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        string lastMigration = null!;
        using (var cmd = new NpgsqlCommand("SELECT name FROM migrations ORDER BY AppliedOn DESC LIMIT 1", connection))
        {
            var result = cmd.ExecuteScalar();
            if(result == null)
            {
                Console.WriteLine("No migrations to rollback.");
                return;
            }
            lastMigration = (string)result;
        }

        var filePath = Path.Combine("Migrations", lastMigration);
        if(!File.Exists(filePath))
        {
            Console.WriteLine($"Migration file {lastMigration} not found.");
            return;
        }

        Console.WriteLine($"Rolling back migration: {lastMigration}");

        var content = File.ReadAllText(filePath);
        int start = content.IndexOf("-- down") + 7; 
        var downSql = content.Substring(start).Trim(); // --down er next line thheke shuru kore file er sesh porjonto.

        using var transaction = connection.BeginTransaction();
        try
        {
            using var cmd = new NpgsqlCommand(downSql, connection, transaction);
            cmd.ExecuteNonQuery();

            using var deleteCmd = new NpgsqlCommand("DELETE FROM migrations WHERE name = @name", connection, transaction);
            deleteCmd.Parameters.AddWithValue("@name", lastMigration);
            deleteCmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Failed to rollback migration {lastMigration}: {ex.Message}");
            transaction.Rollback();
        }
    }

}



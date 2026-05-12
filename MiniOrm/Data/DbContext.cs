using System.Reflection;
using Npgsql;

namespace MiniOrm.Data;

public abstract class DbContext : IDisposable
{
    public NpgsqlConnection Connection { get; }
    protected DbContext(string connectionString)
    {
        Connection = new NpgsqlConnection(connectionString);
        Connection.Open();
        InitializeDbSets();

    }

    private void InitializeDbSets()
    {
        var properties = GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition() == typeof(Dbset<>));


        foreach(var prop in properties)
        {
            var dbset = Activator.CreateInstance(prop.PropertyType, this);
            prop.SetValue(this, dbset);
        }
    }

    public void Dispose() => Connection.Dispose();
}
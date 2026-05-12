using System.Reflection;

using Npgsql;

namespace MiniOrm.Data;
public class Dbset<T> where T : class, new()
{
    private readonly DbContext _context;
    private readonly EntityMetadata _metadata = TypeMapper.GetEntityMetadata<T>();
    public Dbset(DbContext context)
    {
        _context = context;
        _metadata = TypeMapper.GetEntityMetadata<T>();
    }

    public int Insert(T entity)
    {
        var columnToInsert = _metadata.Columns.Where(c => !c.IsPrimaryKey).ToList();

        string columnNames = string.Join(", ", columnToInsert.Select(c => c.ColumnName));
        string paramNames = string.Join(", ", columnToInsert.Select(c => "@" + c.ColumnName));

        string sql = $"INSERT INTO {_metadata.TableName} ({columnNames}) VALUES ({paramNames}) RETURNING {_metadata.PrimaryKeyProperty.Name}";

        using var cmd = new NpgsqlCommand(sql, _context.Connection);

        foreach(var column in columnToInsert) // It loops through the columns that are not primary keys and adds parameters to the SQL command. For example, if you have a Product class with properties Id (primary key), Name, and Price, it will add parameters for Name and Price.
        {
            var value = column.Property.GetValue(entity) ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@" + column.ColumnName, value);
        }

        int newId = (int)cmd.ExecuteScalar(); //ExecuteScalar() runs the SQL and catches that RETURNING id value.
        _metadata.PrimaryKeyProperty.SetValue(entity, newId);

        return newId;
    }

    public IEnumerable<T> GetAll()
    {
        var list = new List<T>();
        string sql = $"SELECT * FROM {_metadata.TableName};";

        using var cmd = new NpgsqlCommand(sql, _context.Connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(MapReaderToEntity(reader));
        }

        return list;
    }

    public T? FindById(int id)
    {
        var prkColumn = _metadata.Columns.First(c => c.IsPrimaryKey);
        string sql = $"SELECT * FROM {_metadata.TableName} WHERE {prkColumn.ColumnName} = @id;";

        using var cmd = new NpgsqlCommand(sql, _context.Connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read()? MapReaderToEntity(reader) : null;
    }


    private T MapReaderToEntity(NpgsqlDataReader reader)
    {
        var entity = new T();
        foreach(var col in _metadata.Columns)
        {
            var value = reader[col.ColumnName];
            col.Property.SetValue(entity, value == DBNull.Value? null : value);

        }

        return entity;
    }


    public void Update(T entity)
    {
        var columnToUpdate = _metadata.Columns.Where(c => !c.IsPrimaryKey).ToList();
        var prkColumn = _metadata.Columns.First(c => c.IsPrimaryKey);

        string setCondition = string.Join(", ", columnToUpdate.Select(col => $"{col.ColumnName} = @{col.ColumnName}"));
        string sql = $"UPDATE {_metadata.TableName} SET {setCondition} WHERE {prkColumn.ColumnName} = @pk;";

        using var cmd = new NpgsqlCommand(sql, _context.Connection);
        foreach(var col in columnToUpdate)
        {
            cmd.Parameters.AddWithValue("@" + col.ColumnName, col.Property.GetValue(entity) ?? DBNull.Value); //pass nullvalue for nullable properties.
        }

        cmd.Parameters.AddWithValue("@pk", prkColumn.Property.GetValue(entity));

        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        var prkColumn = _metadata.Columns.First(c => c.IsPrimaryKey);
        string sql = $"DELETE FROM {_metadata.TableName} WHERE {prkColumn.ColumnName} = @id;";

        using var cmd = new NpgsqlCommand(sql, _context.Connection);
        cmd.Parameters.AddWithValue("@id", id);

        cmd.ExecuteNonQuery();
    }
}


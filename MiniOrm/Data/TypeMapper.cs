//This is the most important part of the metadata logic. It uses reflection to scan my C# classes and build the mapping information that the ORM needs.


using System.Reflection;
using System.Reflection.Metadata;
using MiniOrm.Attributes;
using Npgsql.PostgresTypes;
using ColumnAttribute = MiniOrm.Attributes.ColumnAttribute;
using TableAttribute = MiniOrm.Attributes.TableAttribute;   

namespace MiniOrm.Data;
public static class TypeMapper
{
    public static EntityMetadata GetEntityMetadata<T>() => GetEntityMetadata(typeof(T));
    public static EntityMetadata GetEntityMetadata(Type type)
    {
        var tableAttribute = type.GetCustomAttribute<TableAttribute>();

        if(tableAttribute == null)
        {
            throw new Exception($"Type {type.Name} is missing the [Table] attribute.");
        }

        var metadata = new EntityMetadata
        {
            TableName = tableAttribute.Name
        };

        var properties = type.GetProperties();  //  It grabs every public property in your C# class (e.g., Id, Name, Price)                                                                                             

        foreach(var prop in properties) 
        {
            var columnAttribute = prop.GetCustomAttribute<ColumnAttribute>();
            var primaryKeyAttribute = prop.GetCustomAttribute<PrimaryKeyAttribute>();

            if(columnAttribute == null && primaryKeyAttribute == null)
            {
                continue;
            }

            bool isPrimaryKey = primaryKeyAttribute != null;
            string columnName = isPrimaryKey? primaryKeyAttribute.Name : columnAttribute!.Name; //It determines if the property is the Primary Key and gets the actual column name.

            var columnMetadata = new ColumnMetadata    //It converts C# types to Postgres types.
            {
                ColumnName = columnName,
                IsPrimaryKey = isPrimaryKey,
                Property = prop,
                IsNullable = IsPropertyNullable(prop),
                PostgresTYpe = MapToPostgresType(prop, isPrimaryKey)
            };

            metadata.Columns.Add(columnMetadata);

            if (isPrimaryKey) //To update a row, the ORM needs to know which property to use in the WHERE id = @id clause.
            {
                metadata.PrimaryKeyProperty = prop;
            }
        }
        return metadata;
    }

    private static bool IsPropertyNullable(PropertyInfo prop)
    { 
        if(Nullable.GetUnderlyingType(prop.PropertyType) != null)   //The method Nullable.GetUnderlyingType checks: "Is this type wrapped in a Nullable structure?" If you have int, it returns null. If you have int?, it returns typeof(int), which is not null, so the method returns true (It is nullable).
        {
            return true;
        }

        //  Is this a string? If it is, it uses NullabilityInfoContext to look at the hidden metadata behind the scenes. It checks If the code says public string Name, the state is NotNull. If the code says public string? Name, the state is Nullable.
        return prop.PropertyType == typeof(string) &&
                new NullabilityInfoContext().Create(prop).WriteState == NullabilityState.Nullable;
    }

    private static string MapToPostgresType(PropertyInfo prop, bool isPrimaryKey)
    {
        Type type = prop.PropertyType;
        Type? underLyingType = Nullable.GetUnderlyingType(type);

        bool isNullable = underLyingType != null || type ==typeof(string);

        Type baseType = underLyingType ?? type;

        if(isNullable && baseType == typeof(int) && isPrimaryKey)
        {
            return "SERIAL PRIMARY KEY";
        }

        string pgType = baseType.Name switch
        {
            "Int32" => "INTEGER",
            "Int64" => "BIGINT",
            "Single" => "REAL",
            "Double" => "DOUBLE PRECISION",
            "Decimal" => "NUMERIC",
            "Boolean" => "BOOLEAN",
            "DateTime" => "TIMESTAMP",
            "Guid" => "UUID",
            "String" => "TEXT",
            _ => throw new NotSupportedException($"Type {baseType.Name} is not supported.")
        };

        string nullability = isNullable ? "NULL" : "NOT NULL";

        return $"{pgType} {nullability}";
    }

}






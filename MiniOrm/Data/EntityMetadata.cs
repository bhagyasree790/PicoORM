using System.Reflection;

namespace MiniOrm.Data;

public class EntityMetadata
{
    public string TableName { get; set; } = string.Empty;
    public PropertyInfo PrimaryKeyProperty { get; set; } = null!;  //PropertyInfo is a class in the System.Reflection namespace that represents a property of a class. It contains metadata about the property, such as its name, type, and attributes. In this case, PrimaryKeyProperty will hold the PropertyInfo for the property that is marked as the primary key in the C# class (e.g., Id).

    public List<ColumnMetadata> Columns { get; init; } = new();
}


public class ColumnMetadata
{
    public string ColumnName { get; init; } = string.Empty;
    public string PostgresTYpe { get; init; } = string.Empty;
    public bool IsNullable { get; init; }
    public bool IsPrimaryKey { get; init; }
    public PropertyInfo Property { get; init; } = null!;
}
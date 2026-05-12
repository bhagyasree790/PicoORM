namespace MiniOrm.Attributes;

public class ColumnAttribute : Attribute
{
    public string Name { get; }

    public ColumnAttribute(string name) => Name = name;
}
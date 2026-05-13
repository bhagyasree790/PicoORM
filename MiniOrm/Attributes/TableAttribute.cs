namespace MiniOrm.Attributes;

[AttributeUsage(AttributeTargets.Class)]  //Reflection eitake chinbe
public class TableAttribute : Attribute
{
    public string Name { get; }

    public TableAttribute(string name) => Name = name;
}
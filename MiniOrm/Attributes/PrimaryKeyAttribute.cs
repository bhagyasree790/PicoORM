namespace MiniOrm.Attributes;

public class PrimaryKeyAttribute: ColumnAttribute
{
    public PrimaryKeyAttribute(string name = "id") : base(name)
    {
        
    }
}
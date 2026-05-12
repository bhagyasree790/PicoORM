using MiniOrm.Data;
using MiniOrm.Models;

public class AppDbContext : DbContext
{
    public Dbset<Product> Products {get; set; } = null!;
    public AppDbContext(string connStr) : base(connStr)
    {
        
    }
}
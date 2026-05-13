using MiniOrm.Data;
using MiniOrm.Models;

var connectionString = "MINIORM_CONN";
if(string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Please set the MINIORM_CONN environment variable with your database connection string.");
    return;
}

using var db = new AppDbContext(connectionString);

//Insertion

var product = new Product
{
    Name = "Keyboard",
    Price = 89.99m,
    Discount = null,
    InStock = true
};

db.Products.Insert(product);

Console.WriteLine($"Inserted Product Id = {product.Id}, Discount = {product.Discount}");

//FINDING BY ID
var foundProduct = db.Products.FindById(product.Id);
if(foundProduct != null)
{
    Console.WriteLine($"Found Product: Id={foundProduct.Id}, Name={foundProduct.Name}, Price={foundProduct.Price}, Discount={foundProduct.Discount}, InStock={foundProduct.InStock}");
} else
{
    Console.WriteLine("Product not found.");
}
//UPDATE
if(foundProduct != null)
{
    foundProduct.Price = 79.99m;
    foundProduct.Discount = 5.00m;
    db.Products.Update(foundProduct);
    Console.WriteLine($"Updated Product Id={foundProduct.Id} with new Price={foundProduct.Price} and Discount={foundProduct.Discount}");
}   


//GetAll

var allProducts = db.Products.GetAll();
Console.WriteLine("All Products:");
foreach(var p in allProducts)
{
    Console.WriteLine($"Id={p.Id}, Name={p.Name}, Price={p.Price}, Discount={p.Discount}, InStock={p.InStock}");
}

// Delete

Console.WriteLine($"Delete the id = {product.Id} product");
db.Products.Delete(product.Id);

var remainings = db.Products.GetAll();
Console.WriteLine("Remaining Products:");
foreach(var p in remainings)
{
    Console.WriteLine($"Id={p.Id}, Name={p.Name}, Price={p.Price}, Discount={p.Discount}, InStock={p.InStock}");
}



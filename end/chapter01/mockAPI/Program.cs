using mockAPI.Data;
using mockAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Bogus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseSqlite(connection));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (!context.Products.Any())
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Finance.Amount(50, 2000))
            .RuleFor(p => p.CategoryId, f => f.Random.Int(1, 5));
        var products = productFaker.Generate(10000);
        context.Products.AddRange(products);
        context.SaveChanges();
    }
} 

app.MapGet("/products", async (AppDbContext db) =>
    await db.Products.OrderBy(p => p.Id).Take(10).ToListAsync());

app.MapOpenApi();
if (app.Environment.IsDevelopment()) {
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}
app.Run();

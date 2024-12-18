using CountBy.Data;
using CountBy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Bogus;
using CountBy.Services;

var builder = WebApplication.CreateBuilder(args);

// Create and open a connection that will persist for the application lifetime
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DbContext with the shared connection
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite(connection));
    
builder.Services.AddScoped<IProductReadService, ProductReadService>();

var app = builder.Build();

// Initialize database
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

app.MapControllers();
app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.Run();

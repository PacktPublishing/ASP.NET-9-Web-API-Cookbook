using cookbook.Data;
using cookbook.Models;
using Microsoft.EntityFrameworkCore;
using Bogus;
using cookbook.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMemoryDb"));

builder.Services.AddScoped<IProductsService, ProductReadService>();

builder.Services.AddCors(options => 
{
    options.AddPolicy("CorsPolicy", builder => 
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-Pagination");
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();

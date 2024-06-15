using cookbook.Data;
using cookbook.Models;
using Microsoft.EntityFrameworkCore;
using Bogus;
using cookbook.Services;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = (context) =>
    {
        var httpContext = context.HttpContext;
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["supportContact"] = "support@example.com";

        if (context.ProblemDetails.Status == StatusCodes.Status401Unauthorized)
        {
            context.ProblemDetails.Title = "Unauthorized Access";
            context.ProblemDetails.Detail = "You are not authorized to access this resource.";
        }
        else if (context.ProblemDetails.Status == StatusCodes.Status404NotFound)
        {
            context.ProblemDetails.Title = "Resource Not Found";
            context.ProblemDetails.Detail = "The resource you are looking for was not found.";
        }
        else
        {
            context.ProblemDetails.Title = "An unexpected error occurred";
            context.ProblemDetails.Detail = "An unexpected error occurred. Please try again later.";
        }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMemoryDb"));

builder.Services.AddScoped<IProductsService, ProductReadService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
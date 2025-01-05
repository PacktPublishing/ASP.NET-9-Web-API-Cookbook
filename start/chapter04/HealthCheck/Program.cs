using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Scalar.AspNetCore;
using books.Data;
using books.Services;
using books.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("chapter4");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBooksRepository, BooksRepository>();
builder.Services.AddScoped<IBooksService, BooksService>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

DatabaseSeeder.Initialize(app.Services);

app.Run();

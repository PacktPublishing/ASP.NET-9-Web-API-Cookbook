using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Scalar.AspNetCore;
using events.Models;
using events.Data;
using events.Services;
using events.Repositories;
using Chapter03.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
builder.Services.AddOpenApi("chapter3");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=./Data/SqliteDB.db"));

builder.Services.AddIdentityCore<IdentityUser>() .AddEntityFrameworkStores<AppDbContext>() 
	.AddDefaultTokenProviders();


builder.Services.AddScoped<IEventsRepository, EventsRepository>();
builder.Services.AddScoped<IEventsService, EventsService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<HttpOnlyMiddleware>();

app.UseResponseCaching();
app.MapOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapScalarApiReference();

var connectionStringBuilder = new SqliteConnectionStringBuilder();
connectionStringBuilder.DataSource = "./Data/SqliteDB.db";

app.Run();

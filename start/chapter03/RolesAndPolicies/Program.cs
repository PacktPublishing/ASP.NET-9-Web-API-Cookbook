using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using events.Models;
using events.Data;
using events.Services;
using events.Repositories;
using Chapter03.Middleware;
using System.Text;

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

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

string keyPath = Path.Combine(builder.Environment.ContentRootPath, "jwt-key.txt");
string jwtKey = File.ReadAllText(keyPath).Trim();
Console.WriteLine($"jwtKey is {jwtKey}");

var jwtSettings = new JwtSettings
{
    Key = jwtKey,
    Issuer = builder.Configuration["Jwt:Issuer"],
    Audience = builder.Configuration["Jwt:Audience"],
    ExpirationInMinutes = int.Parse(builder.Configuration["Jwt:ExpirationInMinutes"] ?? "60")
};

if (string.IsNullOrEmpty(jwtSettings.Key) || 
    string.IsNullOrEmpty(jwtSettings.Issuer) || 
    string.IsNullOrEmpty(jwtSettings.Audience))
{
    throw new InvalidOperationException("JWT settings are incomplete. Please check your configuration and jwt-key.txt file.");
}

Console.WriteLine($"JwtSettings created. Key length: {jwtSettings.Key.Length}, Issuer: {jwtSettings.Issuer}, Audience: {jwtSettings.Audience}");

builder.Services.AddSingleton(jwtSettings);

builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtSettings.Key;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpirationInMinutes = jwtSettings.ExpirationInMinutes;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
});

Console.WriteLine($"AddAuthentication: JWT Key length used for IssuerSigningKey: {jwtSettings.Key.Length}");

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

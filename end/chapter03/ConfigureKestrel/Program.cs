using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using Bogus;
using events.Models;
using events.Data;
using events.Services;
using events.Repositories;
using Chapter03.Middleware;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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

builder.Services.AddScoped<IEventsRepository, EventsRepository>();
builder.Services.AddScoped<IEventsService, EventsService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5218, listenOptions =>
    {
        listenOptions.UseHttps(httpsOptions =>
        {
            httpsOptions.ServerCertificateSelector = (connectionContext, name) =>
            {
                var thumbprint = "6DCBA1F5FA9CF189FEB2C806DFA1E436A80FDF45";
                
                using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                
                if (certs.Count > 0)
                {
                    return certs[0];
                }
                else
                {
                    throw new Exception("Certificate not found.");
                }
            };
        });
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<HttpOnlyMiddleware>();

app.UseResponseCaching();
app.MapOpenApi();

app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapScalarApiReference();

var connectionStringBuilder = new SqliteConnectionStringBuilder();
connectionStringBuilder.DataSource = "./Data/SqliteDB.db";

using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
{
    connection.Open();

    var createTableCommand = connection.CreateCommand();
    createTableCommand.CommandText = @"
        CREATE TABLE IF NOT EXISTS EventRegistrations (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            GUID TEXT,
            FullName TEXT,
            Email TEXT,
            EventName TEXT,
            EventDate TEXT,
            DaysAttending INTEGER,
            Notes TEXT
        )";
    createTableCommand.ExecuteNonQuery();

    var checkTableCommand = connection.CreateCommand();
    checkTableCommand.CommandText = "SELECT COUNT(*) FROM EventRegistrations";
    var count = Convert.ToInt64(checkTableCommand.ExecuteScalar());

    if (count == 0)
    {
        var faker = new Faker<EventRegistration>()
            .RuleFor(e => e.GUID, f => Guid.NewGuid())
            .RuleFor(e => e.FullName, f => f.Name.FullName())
            .RuleFor(e => e.Email, f => f.Internet.Email())
            .RuleFor(e => e.EventName, f => f.Lorem.Word())
            .RuleFor(e => e.EventDate, f => f.Date.Future())
            .RuleFor(e => e.DaysAttending, f => f.Random.Int(1, 7))
            .RuleFor(e => e.Notes, f => f.Lorem.Sentence());

        var registrations = faker.Generate(10000);

        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
            INSERT INTO EventRegistrations (GUID, FullName, Email, EventName, EventDate, DaysAttending, Notes)
            VALUES (@GUID, @FullName, @Email, @EventName, @EventDate, @DaysAttending, @Notes)";

        foreach (var registration in registrations)
        {
            insertCommand.Parameters.Clear();
            insertCommand.Parameters.AddWithValue("@GUID", registration.GUID);
            insertCommand.Parameters.AddWithValue("@FullName", registration.FullName);
            insertCommand.Parameters.AddWithValue("@Email", registration.Email);
            insertCommand.Parameters.AddWithValue("@EventName", registration.EventName);
            insertCommand.Parameters.AddWithValue("@EventDate", registration.EventDate.ToString("yyyy-MM-dd"));
            insertCommand.Parameters.AddWithValue("@DaysAttending", registration.DaysAttending);
            insertCommand.Parameters.AddWithValue("@Notes", registration.Notes);
            insertCommand.ExecuteNonQuery();
        }
    }
}

app.Run();

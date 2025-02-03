using Microsoft.EntityFrameworkCore;
using HotChocolate;
using Serilog;
using Serilog.Events;
using Sales.Data;
using Sales.GraphQL;
using Sales.GraphQL.Types;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Starting Sales API");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<OrderType>()
    .AddType<OrderLineType>()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

app.UseSerilogRequestLogging();

using (var scope = app.Services.CreateScope())
{
    DatabaseSeeder.Initialize(scope.ServiceProvider);
}

app.UseCors();
app.MapGraphQL("/graphql");

app.RunWithGraphQLCommands(args);

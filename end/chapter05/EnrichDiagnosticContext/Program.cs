using Microsoft.EntityFrameworkCore;
using Books.Data;
using Books.Services;
using Books.Repositories;
using Books.Middleware;
using Scalar.AspNetCore;
using Serilog;

try {

var builder = WebApplication.CreateBuilder(args);

Log.Information("Starting Web API");

builder.ConfigureLogging();
builder.Services.AddSingleton<DiagnosticContextEnricher>();

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
builder.Services.AddOpenApi("chapter5");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBooksRepository, BooksRepository>();
builder.Services.AddScoped<IBooksService, BooksService>();

var app = builder.Build();

app.UseSerilogRequestLogging(options => 
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) => {
    var enricher = httpContext.RequestServices.GetRequiredService<DiagnosticContextEnricher>();
    enricher.EnrichFromRequest(diagnosticContext, httpContext);

    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms. IP: {ClientIP}, Endpoint: {EndpointName}, Cached: {IsCached}, Query: {QueryParameters}";

    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "");
    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value ?? "");

    };
});


app.UseResponseCaching();
app.MapOpenApi();
app.UseCors();
app.UseRouting();
app.MapControllers();
app.MapScalarApiReference();

using (var scope = app.Services.CreateScope()) 
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseSeeder.Initialize(context);
}

app.Run();
} 
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally 
{
    Log.CloseAndFlush();
}

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpLogging;
using Books.Data;
using Books.Services;
using Books.Repositories;
using Scalar.AspNetCore;
using Serilog;

try {

var builder = WebApplication.CreateBuilder(args);

Log.Information("Starting Web API");

builder.ConfigureLogging();

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
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestMethod 
        | HttpLoggingFields.RequestPath 
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.RequestHeaders
        | HttpLoggingFields.RequestBody
        | HttpLoggingFields.ResponseStatusCode   
        | HttpLoggingFields.ResponseHeaders
        | HttpLoggingFields.ResponseBody;

    logging.RequestHeaders.Add("Accept");
    logging.ResponseHeaders.Add("WWW-Authenticate");
    logging.MediaTypeOptions.AddText("application/javascript");

    if (builder.Environment.IsDevelopment())
    {
        logging.RequestBodyLogLimit = 4096;
        logging.ResponseBodyLogLimit = 4096;
    }
    else
    {
        logging.RequestBodyLogLimit = 1024;
        logging.ResponseBodyLogLimit = 1024;
    }

    logging.CombineLogs = true;
});

var app = builder.Build();

app.UseHttpLogging();
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

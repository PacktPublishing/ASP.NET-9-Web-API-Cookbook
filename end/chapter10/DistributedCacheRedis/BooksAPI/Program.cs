using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Books.Data;
using Books.Services;
using Books.Repositories;

namespace Books.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddRedisClient(connectionName: "cache");

         // Register IDistributedCache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("cache");
        });

        // Register IConnectionMultiplexer
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = builder.Configuration.GetConnectionString("cache");
            return ConnectionMultiplexer.Connect(configuration);
        });

        builder.AddServiceDefaults();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("X-Pagination", "X-Books-Modified", "ETag"); 
            });
        });


        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<IBooksRepository, BooksRepository>();
        builder.Services.AddScoped<IBooksService, BooksService>();

        var app = builder.Build();

        app.UseCors();
        app.UseRouting();
        app.UseWebSockets();
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            DatabaseSeeder.Initialize(scope.ServiceProvider);
        }

        app.Run();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Books.Data;
using Books.Services;
using Books.Repositories;

namespace Books.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<ICacheKeyTracker, CacheKeyTracker>();
        builder.AddRedisClient(connectionName: "cache");
        builder.AddRedisDistributedCache(connectionName: "cache");

        #pragma warning disable EXTEXP0018
        builder.Services.AddHybridCache(options => 
        {
            options.MaximumPayloadBytes = 1024 * 1024;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });
        #pragma warning restore EXTEXP0018

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

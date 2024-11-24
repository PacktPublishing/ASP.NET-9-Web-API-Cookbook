using Microsoft.EntityFrameworkCore;
using Books.Data;
using Books.Services;
using Books.Repositories;
using InventoryService;

namespace Books;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("X-Pagination", "X-Books-Modified"); 
            });
        });

        builder.Services.AddControllers();

        //builder.Services.AddGrpcClient<Inventory.InventoryClient>();
        // In BooksAPI/Program.cs
        builder.Services.AddGrpcClient<Inventory.InventoryClient>(options =>
        {
            options.Address = new Uri("http://localhost:5003");
        })
        .ConfigureChannel(channel =>
        {
            var handler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan
            };
            channel.HttpHandler = handler;
        });

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

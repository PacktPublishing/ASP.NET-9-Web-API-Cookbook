using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Scalar.AspNetCore;
using books.Data;
using books.Services;
using books.Repositories;

namespace books;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi("chapter4");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBooksRepository, BooksRepository>();
        services.AddScoped<IBooksService, BooksService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        app.UseForwardedHeaders();
        app.UseResponseCaching();
        app.UseCors();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapOpenApi();
            endpoints.MapScalarApiReference();
        });

        DatabaseSeeder.Initialize(serviceProvider);

    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using books.Data;
using books.Services;
using books.Repositories;

namespace books;

public class Startup
{
    public IConfiguration Configuration { get; }
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env) {      Configuration = configuration; 
           _env = env; 
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

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBooksRepository, BooksRepository>();
        services.AddScoped<IBooksService, BooksService>();

        services.AddHttpLogging(logging =>
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
            logging.ResponseHeaders.Add("WWW-Authenticate"); logging.MediaTypeOptions.AddText("application/javascript");

            if (_env.IsDevelopment())
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
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        app.UseHttpLogging();

        app.UseResponseCaching();
        app.UseCors();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        DatabaseSeeder.Initialize(serviceProvider);
    }
}

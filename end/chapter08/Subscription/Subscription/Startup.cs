using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog;
using Books.Data;
using Books.Services;
using Books.Repositories;
using Books.GraphQL;

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

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBooksRepository, BooksRepository>();
        services.AddScoped<IBooksService, BooksService>();
        services.AddGraphQLServer()
            .AddQueryType<Query>()
            .AddSubscriptionType<Subscription>()
            .AddInMemorySubscriptions();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
            var actionDescriptor = httpContext.GetEndpoint()?.Metadata
                .GetMetadata<ControllerActionDescriptor>();
            
            if (actionDescriptor != null)
            {
                diagnosticContext.Set("ActionName", actionDescriptor.ActionName);
                diagnosticContext.Set("ControllerName", actionDescriptor.ControllerName);
            }
        };

        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms. Controller: {ControllerName}, Action: {ActionName}";
        });

        app.UseResponseCaching();
        app.UseCors();
        app.UseRouting();
        app.UseWebSockets();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGraphQL();
        });

        DatabaseSeeder.Initialize(serviceProvider);

        
    }
}

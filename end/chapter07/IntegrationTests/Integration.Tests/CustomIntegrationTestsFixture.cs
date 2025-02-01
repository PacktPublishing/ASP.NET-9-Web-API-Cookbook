using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using books;
using books.Data;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;

namespace Tests.Integration;

public class CustomIntegrationTestsFixture : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Create a new SQLite connection that will remain open until the test ends
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open(); // Open the connection to keep the in-memory database alive

                        // Add DbContext using the SQLite in-memory database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            
            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                db.Database.EnsureCreated();
                Utilities.InitializeDatabase(db);
            }
            

        });

        }

        public HttpClient GetClient()
        {
            return CreateClient();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection?.Close();
            }
    }
}

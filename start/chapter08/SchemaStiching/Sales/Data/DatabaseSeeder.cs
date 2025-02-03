using Serilog;
using Serilog.Events;
using Bogus;
using Sales.Models;

namespace Sales.Data;

public static class DatabaseSeeder
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        if (context.Orders.Any())
            return;

        var orderLineFaker = new Faker<OrderLine>()
            .RuleFor(ol => ol.BookId, f => f.Random.Number(1, 100))
            .RuleFor(ol => ol.Quantity, f => f.Random.Number(1, 5))
            .RuleFor(ol => ol.UnitPrice, f => decimal.Parse(f.Commerce.Price()));

        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.CustomerEmail, f => f.Internet.Email())
            .RuleFor(o => o.OrderDate, f => f.Date.Past(2))
            .RuleFor(o => o.Lines, f => orderLineFaker.Generate(f.Random.Number(1, 5)).ToList())
            .RuleFor(o => o.Total, (f, o) => o.Lines.Sum(l => l.Quantity * l.UnitPrice));

        var orders = orderFaker.Generate(50);
        Log.Information("Seeding {Count} orders...", orders.Count());
        context.Orders.AddRange(orders);
        context.SaveChanges();
        Log.Information("Seeding completed.");
    }
}

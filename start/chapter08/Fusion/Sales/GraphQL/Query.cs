using Sales.Data;
using Sales.Models;

namespace Sales.GraphQL;

public class Query
{
    [GraphQLDescription("Get all orders")]
    public IQueryable<Order> GetOrders([Service] SalesDbContext context) =>
        context.Orders.OrderByDescending(o => o.OrderDate);

    [GraphQLDescription("Get a specific order by ID")]
    public async Task<Order?> GetOrderById(
        [Service] SalesDbContext context,
        int id) =>
        await context.Orders.FindAsync(id);

    [GraphQLDescription("Get orders by customer email")]
    public IQueryable<Order> GetOrdersByCustomer(
        [Service] SalesDbContext context,
        string email) =>
        context.Orders.Where(o => o.CustomerEmail == email);
}

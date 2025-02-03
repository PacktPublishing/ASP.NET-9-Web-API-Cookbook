using HotChocolate.Types;
using Sales.Models;

namespace Sales.GraphQL.Types;

public class OrderType : ObjectType<Order>
{
    protected override void Configure(IObjectTypeDescriptor<Order> descriptor)
    {
        descriptor.Description("An order in the system");

        descriptor
            .Field(o => o.Id)
            .Description("The unique identifier of the order");

        descriptor
            .Field(o => o.CustomerEmail)
            .Description("The customer's email address");

        descriptor
            .Field(o => o.OrderDate)
            .Description("When the order was placed");

        descriptor
            .Field(o => o.Total)
            .Description("The total amount of the order");

        descriptor
            .Field(o => o.Lines)
            .Description("The individual line items in the order");
    }
}

using HotChocolate.Types;
using Sales.Models;

namespace Sales.GraphQL.Types;

public class OrderLineType : ObjectType<OrderLine>
{
    protected override void Configure(IObjectTypeDescriptor<OrderLine> descriptor)
    {
        descriptor.Description("A line item in an order");

        descriptor
            .Field(ol => ol.Id)
            .Description("The unique identifier of the order line");

        descriptor
            .Field(ol => ol.BookId)
            .Description("The ID of the book that was ordered");

        descriptor
            .Field(ol => ol.Quantity)
            .Description("The quantity ordered");

        descriptor
            .Field(ol => ol.UnitPrice)
            .Description("The price per unit at time of order");
    }
}

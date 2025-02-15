var builder = DistributedApplication.CreateBuilder(args);

var inventory = builder.AddProject<Projects.InventoryService>("inventory")
    .WithEndpoint(
        endpointName: "grpc",
        callback: endpoint =>
        {
            endpoint.Port = 5003;
            endpoint.UriScheme = "http";
            endpoint.Transport = "http2";
            endpoint.IsProxied = false;
        }
    );

var books = builder.AddProject<Projects.BooksAPI>("booksapi")
    .WithReference(inventory)
    .WithHttpEndpoint(
        name: "api",
        port: 5011,
        isProxied: false
    );

builder.Build().Run();

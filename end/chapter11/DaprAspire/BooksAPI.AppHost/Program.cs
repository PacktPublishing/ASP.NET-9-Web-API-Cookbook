using Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var pubsub = builder.AddDaprPubSub("pubsub");

var inventory = builder.AddProject<Projects.InventoryService>("inventory")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "inventory",
        AppProtocol = builder.ExecutionContext.IsPublishMode ? null : "https",
        AppPort = 5001
    })
    .WithExternalHttpEndpoints()
    .WithReference(pubsub);

var books = builder.AddProject<Projects.BooksAPI>("books")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "books",
        AppProtocol = builder.ExecutionContext.IsPublishMode ? null : "https",
        AppPort = 5011
    })
    .WithExternalHttpEndpoints()  
    .WithReference(pubsub)
    .WithReference(inventory);

if (builder.Configuration.GetValue<string>("DAPR_CLI_PATH") is { } daprCliPath)
{
    builder.Services.Configure<DaprOptions>(options =>
    {
        options.DaprPath = daprCliPath;
    });
}

var daprDashboard = builder.AddExecutable("dapr-dashboard", "dapr", ".", "dashboard")

    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "dapr-dashboard-http", isProxied: false)

    .ExcludeFromManifest();

builder.Build().Run();
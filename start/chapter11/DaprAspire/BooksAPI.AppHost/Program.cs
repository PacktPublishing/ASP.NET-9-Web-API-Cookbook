var builder = DistributedApplication.CreateBuilder(args);

var inventory = builder.AddProject<Projects.InventoryService>("inventory");
var books = builder.AddProject<Projects.BooksAPI>("books")
    .WithReference(inventory);

builder.Build().Run();

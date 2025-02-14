using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var cache = builder.AddRedis("cache");

builder.AddProject<Books>("BookAPI")
       .WithExternalHttpEndpoints()
       .WithReference(cache);

builder.Build().Run();

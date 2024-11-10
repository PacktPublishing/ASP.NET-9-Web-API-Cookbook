using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Books>("BookAPI")
       .WithExternalHttpEndpoints();

builder.Build().Run();

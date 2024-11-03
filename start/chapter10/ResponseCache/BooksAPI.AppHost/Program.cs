using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Books>("booksAPI");

builder.Build().Run();

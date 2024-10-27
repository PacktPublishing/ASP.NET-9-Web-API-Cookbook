var builder = DistributedApplication.CreateBuilder(args);

var naApi = builder.AddProject<Projects.NorthAmerica>("na-api");
var euApi = builder.AddProject<Projects.Europe>("eu-api");
var apApi = builder.AddProject<Projects.AsiaPacific>("ap-api");

builder.AddProject<Projects.YarpGateway>("gateway")
    .WithReference(naApi)
    .WithReference(euApi)
    .WithReference(apApi);

builder.Build().Run();

builder.AddProject<Projects.YarpGateway>("gateway");

builder.Build().Run();

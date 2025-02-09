var builder = DistributedApplication.CreateBuilder(args);

builder.AddContainer("prometheus", "prom/prometheus")
    .WithBindMount("../prometheus", "/etc/prometheus", isReadOnly: true)
    .WithHttpEndpoint(port: 9090, targetPort: 9090);

var naApi = builder.AddProject<Projects.NorthAmerica>("na-api");
var euApi = builder.AddProject<Projects.Europe>("eu-api");
var apApi = builder.AddProject<Projects.AsiaPacific>("ap-api");

builder.AddProject<Projects.YarpGateway>("gateway")
    .WithReference(naApi)
    .WithReference(euApi)
    .WithReference(apApi);

builder.Build().Run();

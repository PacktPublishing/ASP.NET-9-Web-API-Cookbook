using InventoryService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.WebHost.ConfigureKestrel(options =>
{
    var port = builder.Configuration.GetValue<int?>("Ports:gRPC") ?? 5003;
    options.ListenLocalhost(port, o => o.Protocols = 
        Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();
app.MapGrpcService<InventoryServiceImplementation>();
app.MapGrpcReflectionService();


app.Run();

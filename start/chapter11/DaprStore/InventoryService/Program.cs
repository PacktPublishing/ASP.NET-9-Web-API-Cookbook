var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddDaprClient();
var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapControllers();

app.Run();

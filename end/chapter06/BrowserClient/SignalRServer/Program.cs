using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<VoteDbContext>(options =>
        options.UseSqlite("Data Source=./Data/Data.db")
);

builder.Services.AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<VotingHub>("/votingHub");
app.MapFallbackToFile("index.html");

app.MapControllers();
app.UseStaticFiles();


app.Run();

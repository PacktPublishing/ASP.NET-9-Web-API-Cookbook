using Microsoft.EntityFrameworkCore;
using OpinionPoll.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<VoteDbContext>(options =>
        options.UseSqlite("Data Source=./Data/Data.db")
);

builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthorization();

app.MapHub<VotingHub>("/votingHub");
app.MapControllers();

app.Run();

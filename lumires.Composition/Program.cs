using Infrastructure;
using Infrastructure.Persistence;
using lumires.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();

builder.AddApi();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}


app.UseInfrastructure();

app.UseApi();

app.Run();
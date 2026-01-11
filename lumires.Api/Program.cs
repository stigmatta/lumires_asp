using FastEndpoints;
using lumires.Api.Hubs;
using lumires.Api.Infrastructure;
using lumires.Api.Infrastructure.Constants;
using lumires.Api.Infrastructure.Extensions;
using lumires.Api.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("supabaseDB");
builder.Services.AddLumiresAuth(builder.Configuration);
builder.Services.AddLumiresCache(builder.Configuration);
builder.Services.AddLumiresSignalR(builder.Configuration);
builder.Services.AddLumiresSwagger();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("supabaseDB")!)
    .AddRedis(builder.Configuration.GetConnectionString("cache")!);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("Frontend");

if (app.Environment.IsDevelopment()) app.UseLumiresSwagger(app.Environment);

app.UseHttpsRedirection();

app.MapGet("/redis-check", async (IFusionCache cache, IHubContext<NotificationHub> hub) => //TODO remove after
{
    const string key = "final_reliable_test";
    var value = $"Time: {DateTime.Now:T}";

    var result = await cache.GetOrSetAsync(key, _ => Task.FromResult(value), TimeSpan.FromMinutes(10));

    await hub.Clients.All.SendAsync(HubEvents.ReceiveNotification, "Проверка кэша выполнена!");

    return Results.Ok(new
    {
        OriginalValue = value,
        Result = result
    });
});

//TODO JWT AND READ AND WRITE ID AS A CLIENT

app.MapHub<NotificationHub>(HubConstants.Notifications);
app.UseFastEndpoints();
app.Run();
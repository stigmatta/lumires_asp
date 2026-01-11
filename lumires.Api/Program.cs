using FastEndpoints;
using lumires.Api.Features.Notifications;
using lumires.Api.Infrastructure;
using lumires.Api.Infrastructure.Constants;
using lumires.Api.Infrastructure.Extensions;
using lumires.Api.Infrastructure.Hubs;
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

app.UseCors("Frontend");
app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment()) app.UseLumiresSwagger(app.Environment);

app.UseHttpsRedirection();

//TODO remove
app.MapGet("/redis-check", async (IFusionCache cache, IHubContext<NotificationHub, INotificationClient> hub) =>
{
    const string key = "final_reliable_test";
    var value = $"Time: {DateTime.Now:T}";
    var result = await cache.GetOrSetAsync(key, _ => Task.FromResult(value), TimeSpan.FromMinutes(10));

    await hub.Clients.All.ReceiveNotification(new NotificationCommand(
        Type: EventTypes.LikedReview,
        SenderId: "System",
        TargetId: "All",
        CreatedAt: DateTime.UtcNow
    ));

    return Results.Ok(new { Result = result });
});

app.MapLumiresHubs(app.Configuration);
app.UseFastEndpoints();
app.Run();
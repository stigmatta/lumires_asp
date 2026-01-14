using System.Net.Http.Headers;
using FastEndpoints;
using lumires.Api.Infrastructure.Extensions;
using lumires.Api.Infrastructure.Hubs;
using lumires.Api.Infrastructure.Persistence;
using lumires.Api.Infrastructure.Services;
using lumires.Api.Shared.Abstractions;
using lumires.Api.Shared.Models;
using lumires.Api.Shared.Options;
using lumires.ServiceDefaults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("supabaseDB");
builder.Services.AddLumiresAuth(builder.Configuration);
builder.Services.AddLumiresCache(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddLumiresEmail(builder.Configuration, builder.Environment);
builder.Services.AddLumiresSwagger();


builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("supabaseDB")!)
    .AddRedis(builder.Configuration.GetConnectionString("cache")!);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.Configure<TmdbConfig>(
    builder.Configuration.GetSection(TmdbConfig.Section));

builder.Services.AddHttpClient<IExternalMovieService, TmdbService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<TmdbConfig>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.BearerToken);
});

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
        EventTypes.LikedReview,
        "System",
        "All",
        DateTime.UtcNow
    ));

    return Results.Ok(new { Result = result });
});

app.MapGet("/tmdb-test/{id}", async (int id, IExternalMovieService tmdb) =>
{
    var movie = await tmdb.GetMovieDetailsAsync(id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
});

app.MapLumiresHubs(app.Configuration);
app.UseFastEndpoints();
app.Run();
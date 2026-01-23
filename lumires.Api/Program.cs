using System.Diagnostics;
using System.Text.Json;
using FastEndpoints;
using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Options;
using lumires.Api.Infrastructure.Extensions;
using lumires.Api.Infrastructure.Persistence;
using lumires.Api.Infrastructure.Services;
using lumires.Api.Infrastructure.Services.Tmdb;
using lumires.Api.Infrastructure.Services.Watchmode;
using lumires.ServiceDefaults;
using Microsoft.Extensions.Options;
using Polly;
using Refit;

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

var refitSettings = new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    })
};

builder.Services.Configure<TmdbConfig>(
    builder.Configuration.GetSection(TmdbConfig.Section));

builder.Services.AddTransient<TmdbAuthHandler>();
builder.Services.AddRefitClient<ITmdbApi>(refitSettings)
    .ConfigureHttpClient((sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<TmdbConfig>>().Value;
        client.BaseAddress = settings.BaseUrl;
    })
    .AddHttpMessageHandler<TmdbAuthHandler>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));
builder.Services.AddScoped<IExternalMovieService, TmdbService>();


builder.Services.Configure<WatchmodeOptions>(
    builder.Configuration.GetSection(WatchmodeOptions.SectionName));

builder.Services.AddTransient<WatchmodeAuthHandler>();
builder.Services.AddRefitClient<IWatchmodeApi>(refitSettings)
    .ConfigureHttpClient((sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<WatchmodeOptions>>().Value;
        client.BaseAddress = settings.BaseUrl;
    })
    .AddHttpMessageHandler<WatchmodeAuthHandler>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1)));
builder.Services.AddScoped<IStreamingService, WatchmodeService>();

builder.Services.AddLocalization();
var supportedCultures = new[] { "uk-UA", "en-US" };

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
            context.ProblemDetails.Extensions.Add("traceId",
                Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
    };
});

var app = builder.Build();

app.UseCors("Frontend");
app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment()) app.UseLumiresSwagger(app.Environment);

app.UseHttpsRedirection();

app.MapLumiresHubs(app.Configuration);
app.UseFastEndpoints();
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));
app.UseExceptionHandler();

//TODO remove
app.MapGet("/tmdb-test/{id}", async (int id, IExternalMovieService tmdb) =>
{
    var movie = await tmdb.GetMovieDetailsAsync(id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
});

app.Run();
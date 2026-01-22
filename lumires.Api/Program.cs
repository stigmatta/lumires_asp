using System.Net.Http.Headers;
using FastEndpoints;
using lumires.Api.Infrastructure.Extensions;
using lumires.Api.Infrastructure.Persistence;
using lumires.Api.Infrastructure.Services;
using lumires.Api.Infrastructure.Services.Watchmode;
using lumires.Api.Shared.Abstractions;
using lumires.Api.Shared.Options;
using lumires.ServiceDefaults;
using Microsoft.Extensions.Options;

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

builder.Services.Configure<WatchmodeOptions>(
    builder.Configuration.GetSection(WatchmodeOptions.SectionName));

builder.Services.AddHttpClient<IStreamingService, WatchmodeService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<WatchmodeOptions>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl); 
});


builder.Services.AddHttpClient<IExternalMovieService, TmdbService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<TmdbConfig>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.BearerToken);
});

builder.Services.AddLocalization();
var supportedCultures = new[] { "uk-UA", "en-US" };


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


//TODO remove
app.MapGet("/tmdb-test/{id}", async (int id, IExternalMovieService tmdb) =>
{
    var movie = await tmdb.GetMovieDetailsAsync(id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
});

app.Run();




using System.ComponentModel;
using System.Diagnostics;
using FastEndpoints;
using lumires.Api.Core.Abstractions;
using lumires.Api.Infrastructure.Extensions;
using lumires.Api.Infrastructure.Persistence;
using lumires.Api.Infrastructure.Services;
using lumires.ServiceDefaults;
using GlobalExceptionHandler = lumires.Api.Infrastructure.GlobalExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.AddServiceDefaults();
builder.AddLumiresLogging(builder.Configuration);
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
builder.Services.AddExternalApis(builder.Configuration);


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

app.UseHttpsRedirection();

app.MapLumiresHubs(app.Configuration);
app.UseFastEndpoints();
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseLumiresSwagger(app.Environment);
    app.Lifetime.ApplicationStarted.Register(OpenLogtailDashboard);
}

//TODO remove
app.MapGet("/tmdb-test/{id}", async (int id, IExternalMovieService tmdb) =>
{
    var movie = await tmdb.GetMovieDetailsAsync(id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
});



app.Run();
return;


// ---------------------------------------------------------------
static void OpenLogtailDashboard()
{
    try
    {
        const string url = "https://telemetry.betterstack.com/team/t498261/tail?s=1694678";
        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
    catch (Win32Exception ex)
    {
        Console.WriteLine("Failed to open Logtail URL: " + ex.Message);
    }
}
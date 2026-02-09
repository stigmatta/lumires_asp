using Infrastructure;
using lumires.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "..", "lumires.Api", "appsettings.json"),
    false,
    true);

builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "..", "lumires.Api",
        $"appsettings.{builder.Environment.EnvironmentName}.json"),
    true,
    true);

builder.AddInfrastructure();

builder.AddApi();

var app = builder.Build();

app.UseInfrastructure();

app.UseApi();

app.Run();
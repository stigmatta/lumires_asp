using Api;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();

builder.AddApi();

var app = builder.Build();

app.UseInfrastructure();

app.UseApi();

app.Run();
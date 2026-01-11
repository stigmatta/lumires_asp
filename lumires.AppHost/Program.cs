var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddConnectionString("supabaseDB");
// var db = builder.AddPostgres("postgres").AddDatabase("supabaseDB");
var cache = builder.AddConnectionString("cache");


builder.AddProject<Projects.lumires_Api>("api")
    .WithReference(db)
    .WithReference(cache)
    .WithExternalHttpEndpoints();

builder.Build().Run();
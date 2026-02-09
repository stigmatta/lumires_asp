using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddConnectionString("supabaseDB");
// var db = builder.AddPostgres("postgres").AddDatabase("supabaseDB");
// var cache = builder.AddConnectionString("cache");

builder.AddProject<lumires_Composition>("api")
    .WithReference(db)
    .WithExternalHttpEndpoints();

builder.Build().Run();
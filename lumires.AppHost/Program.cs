var builder = DistributedApplication.CreateBuilder(args);

// var db = builder.AddConnectionString("supabaseDB");
var db = builder.AddPostgres("postgres").AddDatabase("supabaseDB");

builder.AddProject<Projects.lumires_Api>("api")
    .WithReference(db); 

builder.Build().Run();
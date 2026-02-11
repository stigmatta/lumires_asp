using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Database
var db = builder.AddConnectionString("supabaseDB");

// Parameters
var supabaseUrl = builder.AddParameter("supabase-url");
var signalRUrl = builder.AddParameter("signalr-url");

// TMDB Configuration
var tmdbBaseUrl = builder.Configuration["TMDB:BaseUrl"] 
    ?? throw new InvalidOperationException("TMDB:BaseUrl is required");
var tmdbImageBaseUrl = builder.Configuration["TMDB:ImageBaseUrl"] 
    ?? throw new InvalidOperationException("TMDB:ImageBaseUrl is required");
var tmdbApiKey = builder.Configuration["TMDB:ApiKey"] 
    ?? throw new InvalidOperationException("TMDB:ApiKey is required");
var tmdbBearer = builder.Configuration["TMDB:BearerToken"] 
    ?? throw new InvalidOperationException("TMDB:BearerToken is required");

// Watchmode Configuration
var watchmodeBaseUrl = builder.Configuration["Watchmode:BaseUrl"] 
    ?? throw new InvalidOperationException("Watchmode:BaseUrl is required");
var watchmodeApiKey = builder.Configuration["Watchmode:ApiKey"] 
    ?? throw new InvalidOperationException("Watchmode:ApiKey is required");

// Cache Settings
var cacheMemoryDuration = builder.Configuration["CacheSettings:MemoryDurationMin"] ?? "5";
var cacheDistributedDuration = builder.Configuration["CacheSettings:DistributedDurationMin"] ?? "20";
var cacheFailSafeMaxDuration = builder.Configuration["CacheSettings:FailSafeMaxDurationHours"] ?? "2";
var cacheFactoryTimeout = builder.Configuration["CacheSettings:FactoryTimeoutMs"] ?? "500";

// Email Settings
var emailFromEmail = builder.Configuration["EmailSender:FromEmail"] 
    ?? throw new InvalidOperationException("EmailSender:FromEmail is required");
var emailFromName = builder.Configuration["EmailSender:FromName"] 
    ?? throw new InvalidOperationException("EmailSender:FromName is required");

builder.AddProject<lumires_Composition>("composition")
    .WithReference(db)
    // Supabase & SignalR
    .WithEnvironment("Supabase__Url", supabaseUrl)
    .WithEnvironment("SignalR__HubUrl", signalRUrl)
    // TMDB
    .WithEnvironment("TMDB__BaseUrl", tmdbBaseUrl)
    .WithEnvironment("TMDB__ImageBaseUrl", tmdbImageBaseUrl)
    .WithEnvironment("TMDB__ApiKey", tmdbApiKey)
    .WithEnvironment("TMDB__BearerToken", tmdbBearer)
    // Watchmode
    .WithEnvironment("Watchmode__BaseUrl", watchmodeBaseUrl)
    .WithEnvironment("Watchmode__ApiKey", watchmodeApiKey)
    // Cache Settings
    .WithEnvironment("CacheSettings__MemoryDurationMin", cacheMemoryDuration)
    .WithEnvironment("CacheSettings__DistributedDurationMin", cacheDistributedDuration)
    .WithEnvironment("CacheSettings__FailSafeMaxDurationHours", cacheFailSafeMaxDuration)
    .WithEnvironment("CacheSettings__FactoryTimeoutMs", cacheFactoryTimeout)
    // Email Sender
    .WithEnvironment("EmailSender__FromEmail", emailFromEmail)
    .WithEnvironment("EmailSender__FromName", emailFromName)
    .WithExternalHttpEndpoints();

builder.Build().Run();
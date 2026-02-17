using Projects;

var builder = DistributedApplication.CreateBuilder(args);


var db = builder.AddConnectionString("db");

var supabaseUrl = builder.AddParameter("supabase-url", true);
var signalRUrl = builder.AddParameter("signalr-url");

// --- TMDB Configuration ---
var tmdbBaseUrl = builder.Configuration["TMDB:BaseUrl"]
                  ?? "https://api.themoviedb.org/3/";

var tmdbImageBaseUrl = builder.Configuration["TMDB:ImageBaseUrl"]
                       ?? "https://image.tmdb.org/t/p/";

var tmdbApiKey = builder.Configuration["TMDB:ApiKey"]
                 ?? throw new InvalidOperationException("TMDB__APIKEY is missing in .env");

var tmdbBearer = builder.Configuration["TMDB:BearerToken"]
                 ?? throw new InvalidOperationException("TMDB__BEARERTOKEN is missing in .env");

var watchmodeBaseUrl = builder.Configuration["Watchmode:BaseUrl"]
                       ?? "https://api.watchmode.com/v1/";

var watchmodeApiKey = builder.Configuration["Watchmode:ApiKey"]
                      ?? throw new InvalidOperationException("WATCHMODE__APIKEY is missing in .env");

var resendApiKey = builder.Configuration["Resend:ApiKey"]
                   ?? throw new InvalidOperationException("RESEND__APIKEY is missing in .env");

var cacheMemoryDuration = builder.Configuration["CacheSettings:MemoryDurationMin"] ?? "5";
var cacheDistributedDuration = builder.Configuration["CacheSettings:DistributedDurationMin"] ?? "20";
var cacheFailSafeMaxDuration = builder.Configuration["CacheSettings:FailSafeMaxDurationHours"] ?? "2";
var cacheFactoryTimeout = builder.Configuration["CacheSettings:FactoryTimeoutMs"] ?? "500";

var emailFromEmail = builder.Configuration["EmailSender:FromEmail"] ?? "no-reply@yourdomain.com";
var emailFromName = builder.Configuration["EmailSender:FromName"] ?? "Lumires App";

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
    // Resend
    .WithEnvironment("Resend__ApiKey", resendApiKey)
    .WithExternalHttpEndpoints();

builder.Build().Run();
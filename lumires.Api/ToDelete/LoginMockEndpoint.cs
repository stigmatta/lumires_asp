using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.ToDelete;

[UsedImplicitly]
internal record AuthRequest(string Email, string Password);

internal record AuthResponse(string Token, string UserId, int ExpiresIn);

internal record SupabaseTokenResponse(string access_token, int expires_in, SupabaseUser user);

[UsedImplicitly]
internal record SupabaseUser(string id);

internal class LoginMockEndpoint(IConfiguration config) : Endpoint<AuthRequest, AuthResponse>, IDisposable
{
    private readonly HttpClient _httpClient = new();

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public override void Configure()
    {
        Post("/api/auth/emulate-login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AuthRequest req, CancellationToken ct)
    {
        var supabaseUrl = config["Supabase:Url"];
        var supabaseKey = config["Supabase:AnonKey"];

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", supabaseKey);

        var response = await _httpClient.PostAsJsonAsync(
            $"{supabaseUrl}/auth/v1/token?grant_type=password",
            new { email = req.Email, password = req.Password },
            ct);


        var data = await response.Content.ReadFromJsonAsync<SupabaseTokenResponse>(ct);

        await Send.OkAsync(new AuthResponse(
            data!.access_token,
            data.user.id,
            data.expires_in
        ), ct);
    }
}
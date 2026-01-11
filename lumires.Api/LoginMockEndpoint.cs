using FastEndpoints;

namespace lumires.Api;


//TODO remove
public record AuthRequest(string Email, string Password);
public record AuthResponse(string Token, string UserId, int ExpiresIn);

internal record SupabaseTokenResponse(string access_token, int expires_in, SupabaseUser user);
internal record SupabaseUser(string id);

public class LoginMockEndpoint(IConfiguration config) : Endpoint<AuthRequest, AuthResponse>
{
    private readonly HttpClient _httpClient = new();

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
            Token: data!.access_token,
            UserId: data.user.id,
            ExpiresIn: data.expires_in
        ), ct);
    }
}
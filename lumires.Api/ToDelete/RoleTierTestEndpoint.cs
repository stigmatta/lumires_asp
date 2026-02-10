using Core.Auth;
using FastEndpoints;

namespace Api.ToDelete;

internal class RoleTierTestEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/role-tier-test");
        Policies(CustomPolicies.StaffOnly);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
    }
}
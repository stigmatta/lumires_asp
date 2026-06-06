using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.GetThisWeekTrendingThreads;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThisWeekTrendingReviews";
        Description = "Get this week trending threads";

        Response(200, "Threads are successfully retrieved");
    }
}
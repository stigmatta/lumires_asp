using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetThisWeekTrendingReviews;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetThisWeekTrendingReviews";
        Description = """
                      Get this week trending reviews

                      ### Notes

                      - **Sending lang is optional**
                      """;

        Response(200, "Reviews are successfully retrieved");
    }
}
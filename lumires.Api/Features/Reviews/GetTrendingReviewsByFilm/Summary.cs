using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetTrendingReviewsByFilm;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "GetTrendingReviewsByFilm";
        Description = """
                      Get trending reviews by a film

                      ### Notes

                      - **Sending lang is optional**
                      """;

        Response(200, "Reviews are successfully retrieved");
    }
}
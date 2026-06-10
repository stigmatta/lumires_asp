using System.Text.RegularExpressions;
using Ardalis.Result;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Tmdb.TmdbAwards;

/// <summary>
///     Sources person award counts by scraping the public TMDB website awards page.
///     TMDB has no awards API; the awards page exposes the aggregate counts in its
///     <c>&lt;meta name="description"&gt;</c> tag, e.g.
///     "...has received 17 nominations and 6 wins.".
/// </summary>
public sealed partial class TmdbAwardsService(
    HttpClient httpClient,
    ILogger<TmdbAwardsService> logger) : IExternalAwardsService
{
    public async Task<Result<PersonAwards>> GetPersonAwardsAsync(int personId, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"person/{personId}/awards", ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to reach TMDB awards page for personId={PersonId}", personId);
            return Result.Error("Failed to fetch awards from TMDB");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.NotFound();

        if (!response.IsSuccessStatusCode)
            return Result.Error("Failed to fetch awards from TMDB");

        var html = await response.Content.ReadAsStringAsync(ct);

        var match = AwardsSummaryRegex().Match(html);

        // Page loaded but the person has no awards listed.
        if (!match.Success)
            return Result.Success(new PersonAwards(0, 0));

        var nominations = int.Parse(match.Groups["nominations"].Value);
        var wins = int.Parse(match.Groups["wins"].Value);

        return Result.Success(new PersonAwards(nominations, wins));
    }

    [GeneratedRegex(
        @"received\s+(?<nominations>\d+)\s+nominations?\s+and\s+(?<wins>\d+)\s+wins?",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AwardsSummaryRegex();
}

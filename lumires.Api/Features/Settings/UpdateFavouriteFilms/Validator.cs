using FastEndpoints;
using FluentValidation;
using lumires.Core.Resources;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Settings.UpdateFavouriteFilms;

internal sealed class Validator : Validator<Command>
{
    public Validator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.FavouriteFilms)
            .Must(x => x.Count <= 4)
            .WithMessage(localizer["UpdateFavoriteFilms_Validation_Error_Length"]);

        RuleForEach(x => x.FavouriteFilms)
            .Must(f => f.Order is >= 1 and <= 4)
            .WithMessage(localizer["UpdateFavoriteFilms_Validation_Error_Order_Length"]);

        RuleFor(x => x.FavouriteFilms)
            .Must(films =>
            {
                var orders = films.Select(f => f.Order).ToList();
                return orders.Distinct().Count() == orders.Count;
            })
            .WithMessage(localizer["UpdateFavoriteFilms_Validation_Error_Order_Unique"]);
    }
}

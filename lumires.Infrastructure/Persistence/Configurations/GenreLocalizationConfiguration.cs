using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class GenreLocalizationConfiguration : IEntityTypeConfiguration<GenreLocalization>
{
    public void Configure(EntityTypeBuilder<GenreLocalization> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();

        builder.Property(g => g.Name).IsRequired().HasMaxLength(StringLimits.Default);

        builder.Property(g => g.LanguageCode).IsRequired().HasMaxLength(StringLimits.Code);

        builder.HasOne(g => g.Genre)
            .WithMany(g => g.Localizations)
            .HasForeignKey(g => g.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
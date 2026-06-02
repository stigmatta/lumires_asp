using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class FilmLocalizationConfiguration : IEntityTypeConfiguration<FilmLocalization>
{
    public void Configure(EntityTypeBuilder<FilmLocalization> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Description).HasMaxLength(StringLimits.Description);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(StringLimits.Default);
        builder.HasIndex(m => m.Title);

        builder.Property(m => m.LanguageCode).IsRequired().HasMaxLength(StringLimits.Code);

        builder.Property(m => m.Tagline).HasMaxLength(StringLimits.Default);

        builder.HasOne(m => m.Film)
            .WithMany(m => m.Localizations)
            .HasForeignKey(m => m.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        // builder.Property(e => e.SearchVector)
        //     .HasColumnType("tsvector")
        //     .HasComputedColumnSql(
        //         """movie_translation_search_vector_update("LanguageCode", "Title", "Description", '')""", 
        //         stored: true);
        // builder.HasIndex(e => e.SearchVector)
        //     .HasMethod("GIN");
    }
}
using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class MovieLocalizationConfiguration : IEntityTypeConfiguration<MovieLocalization>
{
    public void Configure(EntityTypeBuilder<MovieLocalization> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Description).IsRequired().HasMaxLength(StringLimits.Description);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(StringLimits.Default);
        builder.Property(m => m.LanguageCode).IsRequired().HasMaxLength(StringLimits.Code);
        builder.HasOne(m => m.Movie)
            .WithMany(m => m.Localizations)
            .HasForeignKey(m => m.MovieId)
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
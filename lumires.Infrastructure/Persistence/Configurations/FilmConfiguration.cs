using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class FilmConfiguration : IEntityTypeConfiguration<Film>
{
    public void Configure(EntityTypeBuilder<Film> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.HasIndex(m => m.ExternalId).IsUnique();

        builder.Property(m => m.PosterPath).HasMaxLength(StringLimits.Default);

        builder.Property(m => m.BackdropPath).HasMaxLength(StringLimits.Default);

        builder.Property(m => m.TrailerUrl).HasMaxLength(StringLimits.Default);

        builder.HasIndex(m => m.Slug);

        builder.Property(m => m.Slug)
            .IsRequired()
            .HasMaxLength(StringLimits.Default);

        builder.Property(m => m.ProductionCompany)
            .HasMaxLength(StringLimits.Default);

        builder.HasMany(m => m.Genres)
            .WithMany()
            .UsingEntity("FilmGenres");

        builder.HasMany(m => m.Reviews)
            .WithOne(r => r.Film)
            .HasForeignKey(r => r.FilmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
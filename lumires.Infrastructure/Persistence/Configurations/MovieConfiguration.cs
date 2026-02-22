using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.HasIndex(m => m.ExternalId).IsUnique();
        builder.Property(m => m.PosterPath).HasMaxLength(50);
        builder.Property(m => m.BackdropPath).HasMaxLength(50);
        builder.Property(m => m.TrailerUrl).HasMaxLength(50);
    }
}
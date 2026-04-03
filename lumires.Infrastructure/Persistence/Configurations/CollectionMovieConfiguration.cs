using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class CollectionMovieConfiguration : IEntityTypeConfiguration<CollectionMovie>
{
    public void Configure(EntityTypeBuilder<CollectionMovie> builder)
    {
        builder.ToTable("CollectionMovies");

        builder.HasKey(x => new { x.CollectionId, x.MovieId });

        builder.HasIndex(x => new { x.CollectionId, x.Order })
            .IsUnique();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.AddedAt)
            .IsRequired();

        builder.HasOne(x => x.Collection)
            .WithMany(c => c.Movies)
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Movie)
            .WithMany() 
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}
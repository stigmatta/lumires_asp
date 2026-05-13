using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ListFilmConfiguration : IEntityTypeConfiguration<ListFilm>
{
    public void Configure(EntityTypeBuilder<ListFilm> builder)
    {
        builder.ToTable("ListFilms");

        builder.HasKey(x => new { CollectionId = x.FilmsListId, MovieId = x.FilmId });

        builder.HasIndex(x => new { CollectionId = x.FilmsListId, x.Order })
            .IsUnique();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.AddedAt)
            .IsRequired();

        builder.HasOne(x => x.FilmsList)
            .WithMany(c => c.Films)
            .HasForeignKey(x => x.FilmsListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Film)
            .WithMany()
            .HasForeignKey(x => x.FilmId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
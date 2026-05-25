using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserFilmRatingConfiguration : IEntityTypeConfiguration<UserFilmRating>
{
    public void Configure(EntityTypeBuilder<UserFilmRating> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Rating)
            .HasPrecision(2, 1)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasOne(x => x.User)
            .WithMany(x => x.FilmRatings)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Film)
            .WithMany(x => x.UserRatings)
            .HasForeignKey(x => x.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.FilmId })
            .IsUnique();

        builder.ToTable("UserFilmRatings");
    }
}
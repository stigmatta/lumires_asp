using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class FilmsLikeConfiguration : IEntityTypeConfiguration<FilmLike>
{
    public void Configure(EntityTypeBuilder<FilmLike> builder)
    {
        builder.ToTable("FilmsLikes");
        builder.HasKey(x => new { x.FilmId, x.UserId });

        builder.HasOne<Film>()
            .WithMany(r => r.Likes)
            .HasForeignKey(x => x.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
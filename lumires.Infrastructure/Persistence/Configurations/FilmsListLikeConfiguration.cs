using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class FilmsListLikeConfiguration : IEntityTypeConfiguration<FilmsListLike>
{
    public void Configure(EntityTypeBuilder<FilmsListLike> builder)
    {
        builder.ToTable("FilmsListLikes");
        builder.HasKey(x => new { x.FilmsListId, x.UserId });

        builder.HasOne<FilmsList>()
            .WithMany(r => r.Likes)
            .HasForeignKey(x => x.FilmsListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ReviewCommentLikeConfiguration : IEntityTypeConfiguration<ReviewCommentLike>
{
    public void Configure(EntityTypeBuilder<ReviewCommentLike> builder)
    {
        builder.HasKey(x => new { x.ReviewCommentId, x.UserId });

        builder.HasOne<ReviewComment>()
            .WithMany(r => r.Likes)
            .HasForeignKey(x => x.ReviewCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserThreadCommentLikeConfiguration : IEntityTypeConfiguration<UserThreadCommentLike>
{
    public void Configure(EntityTypeBuilder<UserThreadCommentLike> builder)
    {
        builder.HasKey(x => new { x.UserThreadCommentId, x.UserId });

        builder.HasOne<UserThreadComment>()
            .WithMany(r => r.Likes)
            .HasForeignKey(x => x.UserThreadCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
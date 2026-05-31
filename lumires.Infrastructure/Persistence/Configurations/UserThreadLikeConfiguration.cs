using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserThreadLikeConfiguration : IEntityTypeConfiguration<UserThreadLike>
{
    public void Configure(EntityTypeBuilder<UserThreadLike> builder)
    {
        builder.HasKey(x => new { x.ThreadId, x.UserId });

        builder.HasOne<UserThread>()
            .WithMany(r => r.Likes)
            .HasForeignKey(x => x.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
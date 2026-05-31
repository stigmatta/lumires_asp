using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserThreadCommentConfiguration : IEntityTypeConfiguration<UserThreadComment>
{
    public void Configure(EntityTypeBuilder<UserThreadComment> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        builder.Property(x => x.Text).IsRequired().HasMaxLength(StringLimits.Description);

        builder.HasOne(rc => rc.Commentator)
            .WithMany(u => u.UserThreadsComments)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(tc => tc.Thread)
            .WithMany(r => r.UserThreadComments)
            .HasForeignKey(rc => rc.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rc => rc.TargetedUser)
            .WithMany()
            .HasForeignKey(rc => rc.TargetedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
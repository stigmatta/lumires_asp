using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ReviewCommentConfiguration : IEntityTypeConfiguration<ReviewComment>
{
    public void Configure(EntityTypeBuilder<ReviewComment> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.Property(x => x.Text).IsRequired().HasMaxLength(StringLimits.Description);

        builder.HasOne(rc => rc.Commentator)
            .WithMany(u => u.ReviewComments)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(rc => rc.Review)
            .WithMany(r => r.ReviewComments)
            .HasForeignKey(rc => rc.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ReviewTagConfiguration : IEntityTypeConfiguration<ReviewTag>
{
    public void Configure(EntityTypeBuilder<ReviewTag> builder)
    {
        builder.HasKey(x => new { x.ReviewId, x.TagId });

        builder.HasOne(x => x.Review)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        builder.Property(x => x.Title).IsRequired(false)
            .HasMaxLength(StringLimits.Name);

        builder.Property(x => x.Text).IsRequired()
            .HasMaxLength(StringLimits.Description);

        builder.Property(x => x.Rating)
            .HasPrecision(3, 1)
            .IsRequired(false);

        builder
            .HasOne(r => r.Reviewer)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Film)
            .WithMany(m => m.Reviews)
            .HasForeignKey(r => r.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Review.ReviewComments))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
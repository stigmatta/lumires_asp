using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserThreadConfiguration : IEntityTypeConfiguration<UserThread>
{
    public void Configure(EntityTypeBuilder<UserThread> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        builder.Property(x => x.Title).IsRequired(false)
            .HasMaxLength(StringLimits.Name);

        builder.Property(x => x.Text).IsRequired()
            .HasMaxLength(StringLimits.Description);

        builder
            .HasOne(r => r.User)
            .WithMany(u => u.UserThreads)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(UserThread.UserThreadComments))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
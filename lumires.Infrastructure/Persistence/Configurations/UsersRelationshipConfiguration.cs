using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UsersRelationshipConfiguration : IEntityTypeConfiguration<UsersRelationship>
{
    public void Configure(EntityTypeBuilder<UsersRelationship> builder)
    {
        builder.HasOne(x => x.SourceUser)
            .WithMany(x => x.OutgoingRelationships)
            .HasForeignKey(x => x.SourceUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetUser)
            .WithMany(x => x.IncomingRelationships)
            .HasForeignKey(x => x.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(StringLimits.Name);
        
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(StringLimits.Name);
    }
}
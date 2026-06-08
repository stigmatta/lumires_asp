using lumires.Core.Constants;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(StringLimits.Name);

        builder.HasIndex(x => x.Username)
            .IsUnique();
        
        builder.Property(x => x.DisplayName)
            .IsRequired(false)
            .HasMaxLength(StringLimits.Name);
        
       builder.Property(x => x.Pronouns)
            .HasConversion<string>()
            .HasDefaultValue(UserPronouns.NotDefined)
            .HasMaxLength(StringLimits.Code);
       
       builder.Property(x => x.Location)
           .IsRequired(false)
           .HasMaxLength(StringLimits.Name);
       
       builder.Property(x => x.Tagline)
           .IsRequired(false)
           .HasMaxLength(StringLimits.Default);
       
       builder.Property(x => x.Biography)
           .IsRequired(false)
           .HasMaxLength(StringLimits.Biography);

        builder.Property(x => x.Email)
            .HasMaxLength(StringLimits.Name);

        builder.Property(x => x.AvatarUrl)
            .IsRequired(false)
            .HasMaxLength(StringLimits.Url);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasMany(x => x.FilmsLists)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Metadata
            .FindNavigation(nameof(User.FilmsLists))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(User.Reviews))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(User.ReviewComments))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(User.WatchedFilms))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(User.SavedFilms))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(User.SavedLists))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
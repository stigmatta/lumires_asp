using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("UsersSettings");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithOne(u => u.UserSettings)
            .HasForeignKey<UserSettings>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.FavoriteFilms)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserFavoriteFilms"));

        builder.OwnsOne(x => x.Notifications, n =>
        {
            n.Property(p => p.NewFollower).HasColumnName("NotifyNewFollower");
            n.Property(p => p.LikesOnContent).HasColumnName("NotifyLikesOnReviews");
            n.Property(p => p.RepliesAndMentions).HasColumnName("NotifyRepliesAndMentions");
            n.Property(p => p.ActivityFromFollowed).HasColumnName("NotifyActivityFromFollowed");
            n.Property(p => p.SavesOnLists).HasColumnName("NotifySavesOnLists");
            n.Property(p => p.WeeklyDigest).HasColumnName("NotifyWeeklyDigest");
        });

        builder.Property(x => x.ProfileVisibility)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.IsAnyoneCanFollow);
        builder.Property(x => x.IsWatchlistPublic);
        builder.Property(x => x.AreLikesPublic);
        builder.Property(x => x.AreRatingsShowInFeeds);
    }
}
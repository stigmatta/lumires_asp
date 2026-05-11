using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MovieCastConfiguration : IEntityTypeConfiguration<MovieCast>
{
    public void Configure(EntityTypeBuilder<MovieCast> builder)
    {
        builder.HasKey(mc => mc.Id);

        builder.Property(mc => mc.Character)
            .IsRequired()
            .HasMaxLength(StringLimits.Default);

        builder.Property(mc => mc.Order)
            .IsRequired();

        builder.HasOne(mc => mc.Movie)
            .WithMany(m => m.Cast)
            .HasForeignKey(mc => mc.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mc => mc.Person)
            .WithMany(p => p.MovieCasts)
            .HasForeignKey(mc => mc.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
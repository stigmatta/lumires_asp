using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FilmCastConfiguration : IEntityTypeConfiguration<FilmCast>
{
    public void Configure(EntityTypeBuilder<FilmCast> builder)
    {
        builder.HasKey(mc => mc.Id);

        builder.Property(mc => mc.Character)
            .IsRequired()
            .HasMaxLength(StringLimits.Default);

        builder.Property(mc => mc.Order)
            .IsRequired();

        builder.HasOne(mc => mc.Film)
            .WithMany(m => m.Cast)
            .HasForeignKey(mc => mc.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mc => mc.Person)
            .WithMany(p => p.FilmCasts)
            .HasForeignKey(mc => mc.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PersonLocalizationConfiguration : IEntityTypeConfiguration<PersonLocalization>
{
    public void Configure(EntityTypeBuilder<PersonLocalization> builder)
    {
        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.Id)
            .ValueGeneratedNever();

        builder.Property(pl => pl.PersonId)
            .IsRequired();

        builder.Property(pl => pl.LanguageCode)
            .IsRequired()
            .HasMaxLength(StringLimits.Code);

        builder.Property(pl => pl.Name)
            .IsRequired()
            .HasMaxLength(StringLimits.Default);

        builder.HasIndex(pl => new { pl.PersonId, pl.LanguageCode })
            .IsUnique();

        builder.HasOne(pl => pl.Person)
            .WithMany()
            .HasForeignKey(pl => pl.PersonId);
    }
}
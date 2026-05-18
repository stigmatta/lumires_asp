using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PersonDetailConfiguration : IEntityTypeConfiguration<PersonDetail>
{
    public void Configure(EntityTypeBuilder<PersonDetail> builder)
    {
        builder.HasKey(pd => pd.Id);

        builder.Property(pd => pd.Id)
            .ValueGeneratedNever();

        builder.Property(pd => pd.PersonId)
            .IsRequired();

        builder.Property(pd => pd.LanguageCode)
            .IsRequired()
            .HasMaxLength(StringLimits.Code);

        builder.Property(pd => pd.Biography)
            .HasMaxLength(StringLimits.Biography);

        builder.Property(pd => pd.PlaceOfBirth)
            .HasMaxLength(StringLimits.Default);

        builder.Property(pd => pd.ProfilePath)
            .HasMaxLength(StringLimits.Url);

        builder.HasOne(pd => pd.Person)
            .WithOne(p => p.Detail)
            .HasForeignKey<PersonDetail>(pd => pd.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
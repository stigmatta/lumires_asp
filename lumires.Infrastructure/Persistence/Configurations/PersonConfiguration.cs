using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(p => p.ExternalId)
            .IsUnique();

        builder.HasMany(p => p.FilmCasts)
            .WithOne(fc => fc.Person)
            .HasForeignKey(fc => fc.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.FilmDirectors)
            .WithOne(fd => fd.Person)
            .HasForeignKey(fd => fd.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Localizations)
            .WithOne(pl => pl.Person)
            .HasForeignKey(pl => pl.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Detail)
            .WithOne(pd => pd.Person)
            .HasForeignKey<PersonDetail>(pd => pd.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
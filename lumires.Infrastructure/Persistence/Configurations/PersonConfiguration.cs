using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(StringLimits.Default);


        builder.HasMany(p => p.MovieCasts)
            .WithOne(mc => mc.Person)
            .HasForeignKey(mc => mc.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.MovieDirectors)
            .WithOne(md => md.Person)
            .HasForeignKey(md => md.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
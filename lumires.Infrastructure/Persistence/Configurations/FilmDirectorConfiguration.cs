using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FilmDirectorConfiguration : IEntityTypeConfiguration<FilmDirector>
{
    public void Configure(EntityTypeBuilder<FilmDirector> builder)
    {
        builder.HasKey(md => md.Id);

        builder.HasOne(md => md.Film)
            .WithMany(m => m.Directors)
            .HasForeignKey(md => md.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(md => md.Person)
            .WithMany(p => p.FilmDirectors)
            .HasForeignKey(md => md.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
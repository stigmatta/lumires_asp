using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MovieDirectorConfiguration : IEntityTypeConfiguration<MovieDirector>
{
    public void Configure(EntityTypeBuilder<MovieDirector> builder)
    {
        builder.HasKey(md => md.Id);

        builder.HasOne(md => md.Movie)
            .WithMany(m => m.Directors)
            .HasForeignKey(md => md.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(md => md.Person)
            .WithMany(p => p.MovieDirectors)
            .HasForeignKey(md => md.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
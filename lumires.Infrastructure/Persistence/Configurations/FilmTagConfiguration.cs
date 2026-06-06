using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class FilmTagConfiguration : IEntityTypeConfiguration<FilmTag>
{
    public void Configure(EntityTypeBuilder<FilmTag> builder)
    {
        builder.HasKey(x => new { x.FilmId, x.TagId });

        builder.HasOne(x => x.Film)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
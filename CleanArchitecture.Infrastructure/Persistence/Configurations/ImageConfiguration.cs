using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("Images", DbSchemas.Shared);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PublicId)
            .HasMaxLength(EntityConstraints.TokenMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.PublicId);
        builder.HasIndex(x => x.EntityId);

        builder.Property(x => x.Url)
            .IsRequired();

        builder.Property(x => x.SecureUrl)
            .IsRequired();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNet.Domain.Posts;

namespace SocialNet.Infrastructure.DataAcces.Configurations;

public class PostAttachmentConfiguration : IEntityTypeConfiguration<PostAttachment>
{
    public void Configure(EntityTypeBuilder<PostAttachment> builder)
    {
        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.Type)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<AttachmentType>(v)
            );

        builder.Property(a => a.Data)
        .HasColumnType("varbinary(max)");
    }
}

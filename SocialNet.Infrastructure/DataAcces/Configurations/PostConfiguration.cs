using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNet.Domain.Posts;

namespace SocialNet.Infrastructure.DataAcces.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.HomePageUrl).IsRequired(false);

        builder.HasOne(p => p.User)
            .WithMany(p => p.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ParentPost)
            .WithMany(p => p.Replies)
            .HasForeignKey(p => p.ParentPostId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Attachment)
            .WithOne(a => a.Post)
            .HasForeignKey<PostAttachment>(a => a.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

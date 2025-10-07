using SocialNet.Domain.Identity;

namespace SocialNet.Domain.Posts;

public class Post
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string? HomePageUrl { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; }

    public int? ParentPostId { get; set; }
    public virtual Post? ParentPost { get; set; }
    public virtual ICollection<Post>? Replies { get; set; } = new List<Post>();

    public virtual PostAttachment? Attachment { get; set; }
}

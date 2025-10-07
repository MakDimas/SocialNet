namespace SocialNet.Domain.Posts;

public class PostAttachment
{
    public int Id { get; set; }
    public AttachmentType Type { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public byte[] Data { get; set; }
    public int PostId { get; set; }
    public virtual Post Post { get; set; }
}

namespace SocialNet.Core.Dtos.PostDtos;

public class PostResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string? HomePage { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ParentPostId { get; set; }
    public PostAttachmentResponseDto? Attachment { get; set; }
    public List<PostResponseDto>? Replies { get; set; }
}

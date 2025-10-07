namespace SocialNet.Core.Dtos.PostDtos;

public class CreatePostDto
{
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string? HomePageUrl { get; set; }
    public string Text { get; set; }
    public int UserId { get; set; }
    public int? ParentPostId { get; set; }
    public CreatePostAttachmentDto? AttachmentDto { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace SocialNet.Core.Dtos.PostDtos;

public class CreatePostDto
{
    [Required(ErrorMessage = "User name is required")]
    public string UserName { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "User email is required")]
    public string UserEmail { get; set; }
    public string? HomePageUrl { get; set; }

    [MinLength(1)]
    [Required(ErrorMessage = "Text must contains minimum 1 character")]
    public string Text { get; set; }
    public int UserId { get; set; }
    public int? ParentPostId { get; set; }
    public CreatePostAttachmentDto? AttachmentDto { get; set; }
}

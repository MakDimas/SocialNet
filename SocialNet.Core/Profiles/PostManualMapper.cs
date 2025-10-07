using SocialNet.Core.Dtos.PostDtos;
using SocialNet.Domain.Posts;
using System.Net.Mail;

namespace SocialNet.Core.Profiles;

public static class PostManualMapper
{
    public static Post FromCreatePostDtoToPost(this CreatePostDto postDto) =>
        new Post
        {
            UserName = postDto.UserName,
            UserEmail = postDto.UserEmail,
            HomePageUrl = postDto.HomePageUrl,
            Text = postDto.Text,
            CreatedAt = DateTime.Now,
            UserId = postDto.UserId,
            ParentPostId = postDto.ParentPostId,
            Attachment = postDto.AttachmentDto is not null
        ? new PostAttachment
        {
            FileName = postDto.AttachmentDto.FileName,
            ContentType = postDto.AttachmentDto.ContentType,
            FileSize = postDto.AttachmentDto.FileSize,
            Data = postDto.AttachmentDto.Data,
            Type = Enum.TryParse<AttachmentType>(
                       postDto.AttachmentDto.Type,
                       out var parsedType)
                   ? parsedType
                   : default
        }
        : null
        };

    public static PostResponseDto FromPostToPostResponseDto(this Post post) =>
        new PostResponseDto
        {
            Id = post.Id,
            UserName = post.UserName,
            UserEmail = post.UserEmail,
            HomePage = post.HomePageUrl,
            Text = post.Text,
            CreatedAt = post.CreatedAt,
            ParentPostId = post.ParentPostId,
            Attachment = post.Attachment is not null
        ? new PostAttachmentResponseDto
        {
            Id = post.Attachment.Id,
            FileName = post.Attachment.FileName,
            ContentType = post.Attachment.ContentType,
            FileSize = post.Attachment.FileSize,
            Data = post.Attachment.Data
        }
        : null,
            Replies = post.Replies?.OrderByDescending(r => r.CreatedAt).Select(r => r.FromPostToPostResponseDto()).ToList()
        };

    public static List<PostResponseDto> FromPostsToPostResponseDtos(this IEnumerable<Post> posts) =>
        posts.Select(p => p.FromPostToPostResponseDto()).ToList();
}

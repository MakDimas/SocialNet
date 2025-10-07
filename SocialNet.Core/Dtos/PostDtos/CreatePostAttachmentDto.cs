namespace SocialNet.Core.Dtos.PostDtos;

public class CreatePostAttachmentDto
{
    public string Type { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public byte[] Data { get; set; }
}

namespace SocialNet.Core.Dtos.PostDtos;

public class PostAttachmentResponseDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public byte[] Data { get; set; }
}

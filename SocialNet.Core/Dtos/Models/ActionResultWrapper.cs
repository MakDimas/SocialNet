namespace SocialNet.Core.Dtos.Models;

public class ActionResultWrapper<T>
{
    public int Status { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

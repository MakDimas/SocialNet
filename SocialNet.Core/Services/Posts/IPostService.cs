using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Dtos.PostDtos;

namespace SocialNet.Core.Services.Posts;

public interface IPostService
{
    public Task CreatePostAsync(CreatePostDto postDto);
    public Task<(IEnumerable<PostResponseDto>, int)> GetPostsWithPaginationAsync(QueryParameters parameters);
}

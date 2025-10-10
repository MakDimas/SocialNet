using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Dtos.PostDtos;
using SocialNet.Core.Profiles;
using SocialNet.Core.Repositories;
using SocialNet.Domain.Posts;

namespace SocialNet.Core.Services.Posts;

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PostService> _logger;
    private readonly IPostRepository _postRepository;

    public PostService(IUnitOfWork unitOfWork, ILogger<PostService> logger)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _postRepository = unitOfWork.GetRepository<IPostRepository>();
    }

    public async Task CreatePostAsync(CreatePostDto postDto)
    {
        try
        {
            var postToCreate = postDto.FromCreatePostDtoToPost();

            await _postRepository.AddAsync(postToCreate);
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occured while post creating: {ex.Message}");
            throw;
        }
    }

    public async Task<(IEnumerable<PostResponseDto>, int)> GetPostsWithPaginationAsync(QueryParameters parameters)
    {
        var query = _postRepository.AsQueryable().AsNoTracking().Where(p => p.ParentPost == null);

        var totalCount = await query.CountAsync();

        query = query.Include(p => p.Attachment).Include(p => p.Replies).ThenInclude(r => r.Attachment);

        var isDescending =
                    string.Equals(parameters.SortDirection, "Desc", StringComparison.OrdinalIgnoreCase);

        query = parameters.SortBy switch
        {
            nameof(Post.UserName) =>
                isDescending
                    ? query.OrderByDescending(p => p.UserName)
                    : query.OrderBy(p => p.UserName),
            nameof(Post.UserEmail) =>
                isDescending
                    ? query.OrderByDescending(p => p.UserEmail)
                    : query.OrderBy(p => p.UserEmail),
            nameof(Post.CreatedAt) =>
                isDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        query = query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        var resultPosts = query.AsEnumerable().FromPostsToPostResponseDtos();

        return (resultPosts, totalCount);
    }
}

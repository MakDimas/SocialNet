using Microsoft.AspNetCore.Mvc;
using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Dtos.PostDtos;
using SocialNet.Core.Services.Posts;

namespace SocialNet.WebAPI.Controllers;

public class PostController : BaseController
{
    private readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto postDto)
    {
        await _postService.CreatePostAsync(postDto);

        return Ok(new ActionResultWrapper<object>
        {
            Status = StatusCodes.Status200OK,
            Data = null,
            Message = "Post was created successfully"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetPostsWithPagination([FromQuery] QueryParameters parameters)
    {
        var (posts, totalCount) = await _postService.GetPostsWithPaginationAsync(parameters);

        return Ok(new PaginationResult<PostResponseDto>
        {
            Items = posts,
            TotalCount = totalCount
        });
    }
}

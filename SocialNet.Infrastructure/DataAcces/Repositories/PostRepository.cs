using SocialNet.Core.Repositories;
using SocialNet.Domain.Posts;

namespace SocialNet.Infrastructure.DataAcces.Repositories;

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(AppDbContext dbContext) : base(dbContext) { }
}

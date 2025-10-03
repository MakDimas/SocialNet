using SocialNet.Core.Repositories;
using SocialNet.Domain.Identity;

namespace SocialNet.Infrastructure.DataAcces.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext) { }
}

using Microsoft.EntityFrameworkCore;
using SocialNet.Core.Repositories;

namespace SocialNet.Infrastructure.DataAcces.Repositories;

public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _dbContext;
    protected DbSet<TEntity> _dbSet;

    public BaseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }
}


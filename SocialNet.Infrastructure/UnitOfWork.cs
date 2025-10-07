using Microsoft.EntityFrameworkCore;
using SocialNet.Core;
using SocialNet.Core.Repositories;
using SocialNet.Infrastructure.DataAcces;
using SocialNet.Infrastructure.DataAcces.Repositories;

namespace SocialNet.Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _dbContext;
    private IUserRepository _userRepository;
    private IPostRepository _postRepository;

    private bool _disposed = false;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private IUserRepository UserRepository =>
        _userRepository ??= new UserRepository(_dbContext);

    private IPostRepository PostRepository =>
        _postRepository ??= new PostRepository(_dbContext);

    public T GetRepository<T>() where T : class
    {
        return typeof(T) switch
        {
            _ when typeof(T) == typeof(IUserRepository) => (T)UserRepository,
            _ when typeof(T) == typeof(IPostRepository) => (T)PostRepository,

            _ => throw new ArgumentException($"No repository found for type {typeof(T).Name}"),
        };
    }

    public async Task SaveAsync() =>
        await _dbContext.SaveChangesAsync();

    public void Save() =>
        _dbContext.SaveChanges();

    public void Migrate() =>
        _dbContext.Database.Migrate();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

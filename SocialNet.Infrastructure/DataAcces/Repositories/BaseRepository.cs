using Microsoft.EntityFrameworkCore;
using SocialNet.Core.Repositories;
using System.Linq.Expressions;

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

    public async Task AddRangeAsync(IEnumerable<TEntity> entities) =>
        await _dbSet.AddRangeAsync(entities);

    public void AddRange(IEnumerable<TEntity> entities) =>
         _dbSet.AddRange(entities);

    public async Task AddAsync(TEntity entity) =>
        await _dbSet.AddAsync(entity);

    public void Add(TEntity entity) =>
        _dbSet.Add(entity);

    public async Task<IList<TEntity>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public IList<TEntity> GetAll() =>
        _dbSet.ToList();

    public async Task<TEntity> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<TEntity?> GetFirstAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.Where(predicate).FirstOrDefaultAsync();
    }

    public async Task<IList<TEntity>> GetWhereAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.Where(predicate).ToListAsync();
    }

    public IQueryable<TEntity> AsQueryable() =>
        _dbSet.AsQueryable();

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _dbSet.FirstOrDefaultAsync(predicate);

    public TEntity GetFirst(Expression<Func<TEntity, bool>> predicate) =>
        _dbSet.FirstOrDefault(predicate);

    public async Task<IList<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public IList<TEntity> GetWhere(Expression<Func<TEntity, bool>> predicate) =>
        _dbSet.Where(predicate).ToList();

    public void Update(TEntity entity) =>
        _dbSet.Update(entity);

    public void DeleteRange(IEnumerable<TEntity> entities) =>
        _dbSet.RemoveRange(entities);

    public void Delete(TEntity entity) =>
        _dbSet.Remove(entity);
}


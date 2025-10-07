using System.Linq.Expressions;

namespace SocialNet.Core.Repositories;

public interface IRepository<TEntity> where TEntity : class 
{
    public Task AddRangeAsync(IEnumerable<TEntity> entities);
    public void AddRange(IEnumerable<TEntity> entities);
    public Task AddAsync(TEntity entity);
    public void Add(TEntity entity);

    public Task<IList<TEntity>> GetAllAsync();
    public IList<TEntity> GetAll();
    public Task<TEntity> GetByIdAsync(int id);

    public Task<TEntity?> GetFirstAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes);

    public Task<IList<TEntity>> GetWhereAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes);

    public IQueryable<TEntity> AsQueryable();

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate);
    public TEntity GetFirst(Expression<Func<TEntity, bool>> predicate);
    public Task<IList<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate);
    public IList<TEntity> GetWhere(Expression<Func<TEntity, bool>> predicate);

    public void Update(TEntity entity);

    public void DeleteRange(IEnumerable<TEntity> entities);
    public void Delete(TEntity entity);
}

namespace SocialNet.Core;

public interface IUnitOfWork
{
    public T GetRepository<T>() where T : class;
    public Task SaveAsync();
    public void Save();
    public void Migrate();
}

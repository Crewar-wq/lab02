namespace Lab02.Data;

public interface IRepository<T> where T : class, IEntity
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
    Task<T> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
public interface IBookRepository : IRepository<Book>
{
    IEnumerable<Book> FindByAuthor(string authorName);
    IEnumerable<Book> FindByPriceRange(decimal min, decimal max);
    Task<IReadOnlyList<Book>> FindByAuthorAsync(
        string authorName, CancellationToken ct = default);
}
public interface IAuthorRepository : IRepository<Author>
{
    IEnumerable<Author> FindByName(string name);
}
public interface IUnitOfWork : IDisposable
{
    IBookRepository Books { get; }
    IAuthorRepository Authors { get; }
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

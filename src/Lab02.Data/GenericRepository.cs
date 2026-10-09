using Microsoft.EntityFrameworkCore;

namespace Lab02.Data;

// Changes are staged here and committed by UnitOfWork.
public class GenericRepository<T> : IRepository<T>
    where T : class, IEntity
{
    protected readonly LibraryDbContext Context;
    protected DbSet<T> Set => Context.Set<T>();
    protected virtual IQueryable<T> Query => Set;
    public GenericRepository(LibraryDbContext context) => Context = context;

    public T GetById(int id) => Query.SingleOrDefault(entity => entity.Id == id)
        ?? throw new KeyNotFoundException($"{typeof(T).Name} {id} not found");
    public IEnumerable<T> GetAll() => Query.AsNoTracking()
        .OrderBy(entity => entity.Id).ToList();
    public void Add(T entity) => Set.Add(entity);
    public void Update(T entity) => Set.Update(entity);
    public void Delete(int id) => Set.Remove(GetById(id));

    public async Task<T> GetByIdAsync(int id, CancellationToken ct = default) =>
        await Query.SingleOrDefaultAsync(entity => entity.Id == id, ct)
        ?? throw new KeyNotFoundException($"{typeof(T).Name} {id} not found");
    public async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken ct = default) => await Query.AsNoTracking()
        .OrderBy(entity => entity.Id).ToListAsync(ct);
    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
    }
    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Update(entity);
        return Task.CompletedTask;
    }
    public async Task DeleteAsync(int id, CancellationToken ct = default) =>
        Set.Remove(await GetByIdAsync(id, ct));
}

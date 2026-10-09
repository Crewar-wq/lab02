namespace Lab02.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _context;
    public IBookRepository Books { get; }
    public IAuthorRepository Authors { get; }
    public UnitOfWork(LibraryDbContext context)
    {
        _context = context;
        Books = new BookRepository(context);
        Authors = new AuthorRepository(context);
    }
    // EF Core saves the tracked changes atomically in one transaction.
    public int SaveChanges() => _context.SaveChanges();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
    public void Dispose() => _context.Dispose();
}

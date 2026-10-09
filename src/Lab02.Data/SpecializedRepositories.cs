using Microsoft.EntityFrameworkCore;

namespace Lab02.Data;

public sealed class BookRepository(LibraryDbContext context)
    : GenericRepository<Book>(context), IBookRepository
{
    protected override IQueryable<Book> Query => Set.Include(book => book.Author);
    public IEnumerable<Book> FindByAuthor(string authorName) => Query
        .AsNoTracking().Where(book => book.Author.Name.Contains(authorName))
        .OrderBy(book => book.Id).ToList();
    public IEnumerable<Book> FindByPriceRange(decimal min, decimal max)
    {
        if (min < 0 || max < min || max > long.MaxValue / 100m)
            throw new ArgumentOutOfRangeException(nameof(min));
        // Compare exact integer kopecks even for fractional input boundaries.
        var lower = decimal.Ceiling(min * 100m) / 100m;
        var upper = decimal.Floor(max * 100m) / 100m;
        return Query.AsNoTracking()
            .Where(book => book.Price >= lower && book.Price <= upper)
            .OrderBy(book => book.Id).ToList();
    }
    public async Task<IReadOnlyList<Book>> FindByAuthorAsync(
        string authorName, CancellationToken ct = default) => await Query
        .AsNoTracking().Where(book => book.Author.Name.Contains(authorName))
        .OrderBy(book => book.Id).ToListAsync(ct);
}
public sealed class AuthorRepository(LibraryDbContext context)
    : GenericRepository<Author>(context), IAuthorRepository
{
    public IEnumerable<Author> FindByName(string name) => Query.AsNoTracking()
        .Where(author => author.Name.Contains(name))
        .OrderBy(author => author.Id).ToList();
}

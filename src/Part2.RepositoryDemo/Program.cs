using System.Globalization;
using Lab02.Data;
using Microsoft.EntityFrameworkCore;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var path = Path.GetFullPath(args.Length > 0 ? args[0] : "artifacts/library.db");
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
var options = new DbContextOptionsBuilder<LibraryDbContext>()
    .UseSqlite($"Data Source={path}").Options;
var context = new LibraryDbContext(options);
await context.Database.EnsureCreatedAsync();
using var work = new UnitOfWork(context);

Console.WriteLine("=== Create author and book in one Unit of Work ===");
var author = new Author { Name = "Robert Martin" };
var book = new Book { Title = "Clean Code", Price = 990m, Author = author };
work.Authors.Add(author);
work.Books.Add(book);
Console.WriteLine($"Saved entities: {work.SaveChanges()}");
Console.WriteLine("=== Read ===");
foreach (var item in work.Books.GetAll())
    Console.WriteLine($"Id={item.Id} | {item.Title} | {item.Price:F2} | " +
        item.Author.Name);
Console.WriteLine($"By author: {work.Books.FindByAuthor("Martin").Count()}");
Console.WriteLine($"Price 900..1000: " +
    work.Books.FindByPriceRange(900m, 1000m).Count());

Console.WriteLine("=== Update through business service ===");
new BookCatalog(work).ChangePrice(book.Id, 790m);
Console.WriteLine($"Updated price: {work.Books.GetById(book.Id).Price:F2}");
Console.WriteLine("=== Async CRUD ===");
var second = new Book
    { Title = "Clean Architecture", Price = 1200m, AuthorId = author.Id };
await work.Books.AddAsync(second);
await work.SaveChangesAsync();
var loaded = await work.Books.GetByIdAsync(second.Id);
Console.WriteLine($"Async read: {loaded.Title}");
loaded.Price = 1100m;
await work.Books.UpdateAsync(loaded);
await work.SaveChangesAsync();
Console.WriteLine($"Async updated price: {loaded.Price:F2}");
await work.Books.DeleteAsync(book.Id);
await work.SaveChangesAsync();
Console.WriteLine($"Books after delete: {(await work.Books.GetAllAsync()).Count}");
Console.WriteLine("SQLite database saved");

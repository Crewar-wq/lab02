using Lab02.Data;
using Lab02.Interfaces;
using Lab02.UsersApi;
using Microsoft.EntityFrameworkCore;

var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException("FAILED: " + name);
    Console.WriteLine("PASS: " + name);
    passed++;
}
void Throws<T>(Action action, string name) where T : Exception
{
    try { action(); }
    catch (T) { Check(true, name); return; }
    throw new InvalidOperationException("FAILED: " + name);
}
var point = new Point(1, 2);
((IMovable)point).Move(-3, 7);
Check(point.X == -3 && point.Y == 7, "Move changes coordinates");
IShape circle = new Circle(5);
Check(Math.Abs(circle.GetArea() - 25 * Math.PI) < 1e-9, "Circle area");
Check(Math.Abs(circle.GetPerimeter() - 10 * Math.PI) < 1e-9, "Circle perimeter");
IShape rectangle = new Rectangle(4, 6);
Check(rectangle.GetArea() == 24 && rectangle.GetPerimeter() == 20,
    "Rectangle area and perimeter");
I3DShape cube = new Cube(3);
Check(cube.GetArea() == 54 && cube.GetPerimeter() == 36 &&
    cube.GetVolume() == 27, "Cube formulas");
Throws<ArgumentOutOfRangeException>(() => new Circle(0), "Reject zero radius");
Throws<ArgumentOutOfRangeException>(() => new Cube(double.NaN), "Reject NaN edge");
Throws<ArgumentOutOfRangeException>(() => new Cash().Pay(-1), "Reject negative payment");
Check(new Printer() is IPrinter &&
    !typeof(IScanner).IsAssignableFrom(typeof(Printer)), "Printer follows ISP");
Check(new MultifunctionDevice() is IPrinter and IScanner and IFax,
    "MFD implements all device contracts");

var folder = Path.Combine(Path.GetTempPath(), "lab02-" + Guid.NewGuid());
Directory.CreateDirectory(folder);
try
{
    var logFile = Path.Combine(folder, "work.log");
    Demo.DoWork(new FileLogger(logFile));
    Check(File.ReadAllLines(logFile).SequenceEqual(
        new[] { "Work started", "Work completed" }), "File logger writes messages");
    var options = new DbContextOptionsBuilder<LibraryDbContext>()
        .UseSqlite($"Data Source={Path.Combine(folder, "library.db")}").Options;
    var context = new LibraryDbContext(options);
    context.Database.EnsureCreated();
    using (var work = new UnitOfWork(context))
    {
        var author = new Author { Name = "Robert Martin" };
        var book = new Book { Title = "Clean Code", Price = 990m, Author = author };
        work.Authors.Add(author);
        work.Books.Add(book);
        Check(work.SaveChanges() == 2 && book.AuthorId == author.Id,
            "One Unit of Work saves related entities");
        Check(work.Books.GetById(book.Id).Title == "Clean Code", "Sync read");
        Check(work.Books.FindByAuthor("Martin").Count() == 1, "Author search");
        Check(work.Books.FindByPriceRange(990m, 990m).Count() == 1,
            "Price range includes both boundaries");
        Check(!work.Books.FindByPriceRange(990.01m, 1000m).Any(),
            "Price range excludes lower priced books");
        Check(!work.Books.FindByPriceRange(990.001m, 1000m).Any(),
            "Fractional range boundary does not round down");
        Throws<ArgumentOutOfRangeException>(() =>
            work.Books.FindByPriceRange(100m, 50m), "Reject reversed price range");
        var catalog = new BookCatalog(work);
        catalog.ChangePrice(book.Id, 790.15m);
        context.ChangeTracker.Clear();
        Check(work.Books.GetById(book.Id).Price == 790.15m,
            "Exact monetary value survives SQLite roundtrip");
        Throws<ArgumentOutOfRangeException>(() => catalog.ChangePrice(book.Id, 1.001m),
            "Business service rejects fractional kopecks");
        var second = new Book { Title = "Clean Architecture", Price = 1200m,
            AuthorId = author.Id };
        await work.Books.AddAsync(second);
        await work.SaveChangesAsync();
        var loaded = await work.Books.GetByIdAsync(second.Id);
        Check(loaded.Title == second.Title, "Async create and read");
        loaded.Price = 1100m;
        await work.Books.UpdateAsync(loaded);
        await work.SaveChangesAsync();
        context.ChangeTracker.Clear();
        Check((await work.Books.GetByIdAsync(second.Id)).Price == 1100m,
            "Async update persists");
        Check((await work.Books.FindByAuthorAsync("Martin")).Count == 2,
            "Async specialized search");
        await work.Books.DeleteAsync(second.Id);
        await work.SaveChangesAsync();
        Check((await work.Books.GetAllAsync()).Count == 1, "Async delete");
        work.Books.Delete(book.Id);
        work.SaveChanges();
        Check(!work.Books.GetAll().Any(), "Sync delete");
        Throws<KeyNotFoundException>(() => work.Books.GetById(book.Id),
            "Missing entity reports not found");
    }

    // A failed second insert must also roll back the first repository's insert.
    var rollbackOptions = new DbContextOptionsBuilder<LibraryDbContext>()
        .UseSqlite($"Data Source={Path.Combine(folder, "rollback.db")}").Options;
    var rollbackContext = new LibraryDbContext(rollbackOptions);
    rollbackContext.Database.EnsureCreated();
    using (var work = new UnitOfWork(rollbackContext))
    {
        work.Authors.Add(new Author { Name = "Must be rolled back" });
        work.Books.Add(new Book { Title = "Invalid foreign key", Price = 10m,
            AuthorId = 999 });
        Throws<DbUpdateException>(() => work.SaveChanges(),
            "Foreign key violation aborts Unit of Work");
    }
    using (var verify = new LibraryDbContext(rollbackOptions))
        Check(!verify.Authors.Any() && !verify.Books.Any(),
            "Failed Unit of Work leaves no partial inserts");

    Check(UserValidation.Validate(new("student", new string('a', 64))) is null,
        "Valid user request");
    Check(UserValidation.Validate(new("student", "plain_password")) is not null,
        "Reject plaintext password field");
    Check(UserValidation.Validate(new("a", new string('a', 64))) is not null,
        "Reject short login");
    Check(UserValidation.Validate(new("student", null)) is not null,
        "Reject missing hash");
}
finally
{
    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
    Directory.Delete(folder, recursive: true);
}
Console.WriteLine($"CHECKS PASSED: {passed}");

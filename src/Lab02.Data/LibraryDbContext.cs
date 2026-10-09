using Microsoft.EntityFrameworkCore;

namespace Lab02.Data;

public sealed class LibraryDbContext(
    DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Author>(entity =>
        {
            entity.ToTable("Authors");
            entity.HasKey(author => author.Id);
            entity.Property(author => author.Name)
                .IsRequired().HasMaxLength(100);
        });
        model.Entity<Book>(entity =>
        {
            entity.ToTable("Books", table =>
                table.HasCheckConstraint("CK_Books_Price", "Price >= 0"));
            entity.HasKey(book => book.Id);
            entity.Property(book => book.Title)
                .IsRequired().HasMaxLength(200);
            // Exact money storage: integer kopecks, decimal in C#.
            entity.Property(book => book.Price).HasConversion<long>(
                price => checked((long)(price * 100m)),
                kopecks => kopecks / 100m);
            entity.HasOne(book => book.Author)
                .WithMany(author => author.Books)
                .HasForeignKey(book => book.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

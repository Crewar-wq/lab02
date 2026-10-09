using Microsoft.EntityFrameworkCore;

namespace Lab02.UsersApi;

public sealed class User
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string PassHash { get; set; } = "";
}
public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Login).IsRequired()
                .HasMaxLength(50).UseCollation("NOCASE");
            entity.HasIndex(user => user.Login).IsUnique();
            entity.Property(user => user.PassHash).IsRequired().HasMaxLength(64);
        });
    }
}

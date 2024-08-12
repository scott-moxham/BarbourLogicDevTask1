using Microsoft.EntityFrameworkCore;

namespace Backend.EntityFramework;

public class LibraryDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    public DbSet<User> Users { get; set; }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Backend.tests.TestHelpers;

public abstract class DbTestHelper<T> : IDisposable where T : DbContext
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<T> _options;

    public DbTestHelper()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<T>()
            .UseSqlite(_connection)
            .Options;

        using T dbContext = NewContext();
        dbContext.Database.EnsureCreated();
    }

    public T NewContext()
    {
        return Activator.CreateInstance(typeof(T), _options) as T
            ?? throw new InvalidOperationException("No constructor accepting DbContextOptions<T> available");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _disposed;
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _connection.Dispose();

            _disposed = true;
        }
    }
}
using AutoMapper;

using Backend.EntityFramework;
using Backend.Exceptions;
using Backend.Infrastructure;
using Backend.Models;
using Backend.Services;
using Backend.tests.TestHelpers;

using Microsoft.EntityFrameworkCore;

namespace Backend.tests.Services;

public class BookServiceTests : IDisposable, IAsyncLifetime
{
    private readonly BookService service;
    private readonly LibraryDbHelper dbHelper;
    private readonly IMapper mapper;

    public BookServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(AutoMapperProfile));
        });
        mapper = config.CreateMapper();

        dbHelper = new();

        service = new BookService(dbHelper.NewContext(), mapper);
    }

    public void Dispose()
    {
        dbHelper.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task InitializeAsync()
    {
        await SeedBooks();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private readonly Dictionary<int, Book> efBooks = new() {
        { 1, new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234567890", Availability = Availability.Available } },
        { 2, new Book { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "0987654321", Availability = Availability.CheckedOut } }
    };

    private readonly Dictionary<int, BookDTO> dtoBooks = new() {
        { 1, new BookDTO { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234567890", Availability = Availability.Available } },
        { 2, new BookDTO { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "0987654321", Availability = Availability.CheckedOut } }
    };

    private async Task SeedBooks()
    {
        var dbContext = dbHelper.NewContext();
        dbContext.Books.AddRange(efBooks.Values);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfBookDTOs()
    {
        // Arrange

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(dtoBooks.Values);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnBookDTO()
    {
        // Arrange
        const int id = 1;

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        result.Should().BeEquivalentTo(dtoBooks[id]);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange

        // Act
        var result = await service.GetByIdAsync(5555);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_ShouldReturnAddedBookDTO()
    {
        // Arrange
        var bookAddDTO = new BookAddDTO { Title = "Book 3", Author = "Author 3", ISBN = "555555" };

        // Act
        var result = await service.AddAsync(bookAddDTO);

        // Assert
        result.Should().BeEquivalentTo(bookAddDTO);
        result.Id.Should().BePositive();

        var dbContext = dbHelper.NewContext();
        var bookSaved = await dbContext.Books.FindAsync(result.Id);
        bookSaved.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task Update_WithValidId_ShouldReturnUpdatedBookDTO()
    {
        // Arrange
        var book = efBooks[1];
        const string newTitle = "New Title";
        const string newAuthor = "New Author";
        const string newISBN = "6666666";
        var bookEditDTO = new BookEditDTO(book.Id!.Value, newTitle, newAuthor, newISBN);

        // Act
        var result = await service.UpdateAsync(bookEditDTO);

        // Assert
        result.Should().BeEquivalentTo(bookEditDTO);

        var dbContext = dbHelper.NewContext();
        var bookSaved = await dbContext.Books.FindAsync(book.Id);
        bookSaved.Should().BeEquivalentTo(bookEditDTO);
        bookSaved!.Availability.Should().Be(book.Availability);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var book = efBooks[1];
        const string newTitle = "New Title";
        const string newAuthor = "New Author";
        const string newISBN = "6666666";
        var bookEditDTO = new BookEditDTO(7777, newTitle, newAuthor, newISBN);

        // Act
        var fn = () => service.UpdateAsync(bookEditDTO);

        // Assert
        await fn.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteBook()
    {
        // Arrange
        const int id = 1;

        // Act
        await service.DeleteAsync(id);

        // Assert
        var dbContext = dbHelper.NewContext();
        var book = await dbContext.Books.FindAsync(id);
        book.Should().BeNull();

        var count = await dbContext.Books.CountAsync();
        count.Should().Be(efBooks.Count - 1);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        const int id = 555;

        // Act
        var fn = () => service.DeleteAsync(id);

        // Assert
        await fn.Should().ThrowAsync<NotFoundException>();

        var dbContext = dbHelper.NewContext();
        var count = await dbContext.Books.CountAsync();
        count.Should().Be(efBooks.Count);
    }
}

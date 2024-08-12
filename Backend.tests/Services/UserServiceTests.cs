using AutoMapper;

using Backend.EntityFramework;
using Backend.Exceptions;
using Backend.Infrastructure;
using Backend.Models;
using Backend.Services;
using Backend.tests.TestHelpers;

using Microsoft.EntityFrameworkCore;

namespace Backend.tests.Services;

public class UserServiceTests : IDisposable, IAsyncLifetime
{
    private readonly UserService service;
    private readonly LibraryDbHelper dbHelper;
    private readonly IMapper mapper;

    public UserServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(AutoMapperProfile));
        });
        mapper = config.CreateMapper();

        dbHelper = new();

        service = new UserService(dbHelper.NewContext(), mapper);
    }

    public void Dispose()
    {
        dbHelper.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task InitializeAsync()
    {
        await SeedUsers();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private readonly Dictionary<int, User> efUsers = new() {
        { 1, new User { Id = 1, Forename = "Bob", Surname = "Smith" } },
        { 2, new User { Id = 2, Forename = "Brian", Surname = "Jones" } }
    };

    private readonly Dictionary<int, UserDTO> dtoUsers = new() {
        { 1, new UserDTO { Id = 1, Forename = "Bob", Surname = "Smith" } },
        { 2, new UserDTO { Id = 2, Forename = "Brian", Surname = "Jones" } }
    };

    private async Task SeedUsers()
    {
        var dbContext = dbHelper.NewContext();
        dbContext.Users.AddRange(efUsers.Values);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfUserDTOs()
    {
        // Arrange

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(dtoUsers.Values);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnUserDTO()
    {
        // Arrange
        const int id = 1;

        var book1 = new Book { Id = 1, Title = "Book 1", Author = "Jack Essex", ISBN = "888888" };
        var book2 = new Book { Id = 2, Title = "Book 2", Author = "Nigel Lee", ISBN = "99999" };

        var book1DTO = new BookDTO { Id = 1, Title = "Book 1", Author = "Jack Essex", ISBN = "888888" };
        var book2DTO = new BookDTO { Id = 2, Title = "Book 2", Author = "Nigel Lee", ISBN = "99999" };

        var expectedUser = new UserDTOWithBooks { Id = 1, Forename = "Bob", Surname = "Smith", Books = [book1DTO, book2DTO] };

        var dbContext = dbHelper.NewContext();
        dbContext.Books.AddRange(book1, book2);
        var user = dbContext.Users.Find(id);
        user!.Books = [book1, book2];
        await dbContext.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
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
    public async Task Add_ShouldReturnAddedUserDTO()
    {
        // Arrange
        var userAddDTO = new UserAddDTO { Forename = "Geoff", Surname = "Smith" };

        // Act
        var result = await service.AddAsync(userAddDTO);

        // Assert
        result.Should().BeEquivalentTo(userAddDTO);
        result.Id.Should().BePositive();

        var dbContext = dbHelper.NewContext();
        var userSaved = await dbContext.Users.FindAsync(result.Id);
        userSaved.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task Update_WithValidId_ShouldReturnUpdatedUserDTO()
    {
        // Arrange
        var user = efUsers[1];
        const string newForename = "Nigel";
        const string newSurname = "Jones";
        var userEditDTO = new UserEditDTO(user.Id!.Value, newForename, newSurname);

        // Act
        var result = await service.UpdateAsync(userEditDTO);

        // Assert
        result.Should().BeEquivalentTo(userEditDTO);

        var dbContext = dbHelper.NewContext();
        var userSaved = await dbContext.Users.FindAsync(user.Id);
        userSaved.Should().BeEquivalentTo(userEditDTO);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var user = efUsers[1];
        const string newForename = "Nigel";
        const string newSurname = "Jones";
        var userEditDTO = new UserEditDTO(7777, newForename, newSurname);

        // Act
        var fn = () => service.UpdateAsync(userEditDTO);

        // Assert
        await fn.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteUser()
    {
        // Arrange
        const int id = 1;

        // Act
        await service.DeleteAsync(id);

        // Assert
        var dbContext = dbHelper.NewContext();
        var user = await dbContext.Users.FindAsync(id);
        user.Should().BeNull();

        var count = await dbContext.Users.CountAsync();
        count.Should().Be(efUsers.Count - 1);
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
        var count = await dbContext.Users.CountAsync();
        count.Should().Be(efUsers.Count);
    }

    [Fact]
    public async Task Borrow_WithNoBorrowed_ShouldAddBookToUser()
    {
        // Arrange
        const int userId = 1;

        var book = new Book { Title = "Book 1", Author = "Jack Essex", ISBN = "888888", Availability = Availability.Available };

        var arrangeContext = dbHelper.NewContext();
        arrangeContext.Books.Add(book);
        await arrangeContext.SaveChangesAsync();

        // Act
        await service.BorrowAsync(userId, book.Id!.Value);

        // Assert
        var assertContext = dbHelper.NewContext();
        var user = await assertContext.Users
            .Include(x => x.Books)
            .SingleOrDefaultAsync(x => x.Id == userId);

        user!.Books.Should().HaveCount(1);
        user.Books!.First().Should().BeEquivalentTo(book, opt => opt.Excluding(x => x.BorrowedBy).Excluding(x => x.BorrowedById).Excluding(x => x.Availability));
        user.Books!.First().Availability.Should().Be(Availability.CheckedOut);
    }

    [Fact]
    public async Task Borrow_WithOtherBorrowed_ShouldAddBookToUser()
    {
        // Arrange
        const int userId = 1;

        var existingBook = new Book { Title = "Book 2", Author = "Fred England", ISBN = "9999", Availability = Availability.CheckedOut };
        var newBook = new Book { Title = "Book 1", Author = "Jack Essex", ISBN = "888888", Availability = Availability.Available };

        var arrangeContext = dbHelper.NewContext();
        arrangeContext.Books.Add(existingBook);
        arrangeContext.Books.Add(newBook);

        var userArrange = arrangeContext.Users.Find(userId);
        userArrange!.Books = [existingBook];

        await arrangeContext.SaveChangesAsync();

        // Act
        await service.BorrowAsync(userId, newBook.Id!.Value);

        // Assert
        var assertContext = dbHelper.NewContext();
        var user = await assertContext.Users
            .Include(x => x.Books)
            .SingleOrDefaultAsync(x => x.Id == userId);

        user!.Books.Should().HaveCount(2);
        var book = user.Books!.Single(x => x.Id == newBook.Id);
        book.Should().BeEquivalentTo(newBook, opt => opt.Excluding(x => x.BorrowedBy).Excluding(x => x.BorrowedById).Excluding(x => x.Availability));
        book.Availability.Should().Be(Availability.CheckedOut);

        user.Books!.Single(x => x.Id == existingBook.Id).Should().BeEquivalentTo(existingBook, opt => opt.Excluding(x => x.BorrowedBy).Excluding(x => x.BorrowedById));
    }

    [Fact]
    public async Task Borrow_WithInvalidUserId_ShouldThrowNotFoundException()
    {
        // Arrange
        const int userId = 555;

        var book = new Book { Title = "Book 1", Author = "Jack Essex", ISBN = "888888", Availability = Availability.Available };

        var arrangeContext = dbHelper.NewContext();
        arrangeContext.Books.Add(book);
        await arrangeContext.SaveChangesAsync();

        // Act
        var fn = () => service.BorrowAsync(userId, book.Id!.Value);

        // Assert
        await fn.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Borrow_WithInvalidBookId_ShouldThrowNotFoundException()
    {
        // Arrange
        const int userId = 1;
        const int bookId = 11111;

        // Act
        var fn = () => service.BorrowAsync(userId, bookId);

        // Assert
        await fn.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Borrow_BookAlreadyCheckedOut_ShouldThrowNotAvailableException()
    {
        // Arrange
        const int userId = 1;

        var book = new Book { Title = "Book 1", Author = "Jack Essex", ISBN = "888888", Availability = Availability.CheckedOut };

        var arrangeContext = dbHelper.NewContext();
        arrangeContext.Books.Add(book);
        await arrangeContext.SaveChangesAsync();

        // Act
        var fn = () => service.BorrowAsync(userId, book.Id!.Value);

        // Assert
        await fn.Should().ThrowAsync<BookNotAvailableException>();
    }

    [Fact]
    public async Task Return_Successful()
    {
        // Arrange
        const int userId = 1;

        var book = new Book { Title = "Book 1", Author = "Jack Essex", ISBN = "888888", Availability = Availability.CheckedOut };

        var arrangeContext = dbHelper.NewContext();
        arrangeContext.Books.Add(book);

        var user = arrangeContext.Users.Find(userId);
        user!.Books = [book];
        await arrangeContext.SaveChangesAsync();

        // Act
        await service.ReturnAsync(book.Id!.Value);

        // Assert
        var assertContext = dbHelper.NewContext();
        var bookReturned = await assertContext.Books.FindAsync(book.Id);
        bookReturned!.BorrowedById.Should().BeNull();
        bookReturned.Availability.Should().Be(Availability.Available);
    }

    [Fact]
    public async Task Return_NotFound()
    {
        // Arrange
        const int bookId = 1;

        // Act
        var fn = () => service.ReturnAsync(bookId);

        // Assert
        await fn.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Return_NotCheckedOut()
    {
        // Arrange
        var book = new Book { Title = "Book 1", Author = "Jack Essex", ISBN = "888888", Availability = Availability.Available };

        var arrangeContext = dbHelper.NewContext();
        arrangeContext.Books.Add(book);
        await arrangeContext.SaveChangesAsync();

        // Act
        var fn = () => service.ReturnAsync(book.Id!.Value);

        // Assert
        await fn.Should().ThrowAsync<BookNotCheckedOutException>();
    }
}

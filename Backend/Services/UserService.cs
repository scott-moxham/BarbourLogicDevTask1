using AutoMapper;

using Backend.EntityFramework;
using Backend.Exceptions;
using Backend.Models;

using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserService : IUserService
{
    private readonly LibraryDbContext dbContext;
    private readonly IMapper mapper;

    public UserService(LibraryDbContext dbContext, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }

    public Task<List<UserDTO>> GetAllAsync()
    {
        return dbContext.Users
            .OrderBy(x => x.Surname)
            .ThenBy(x => x.Forename)
            .Select(x => mapper.Map<UserDTO>(x))
            .ToListAsync();
    }

    public async Task<UserDTOWithBooks?> GetByIdAsync(int id)
    {
        var user = await dbContext.Users
            .Include(x => x.Books)
            .SingleOrDefaultAsync(x => x.Id == id);

        return user is null
            ? null
            : mapper.Map<UserDTOWithBooks>(user);
    }

    public async Task<UserDTO> AddAsync(UserAddDTO user)
    {
        var efUser = mapper.Map<User>(user);

        dbContext.Users.Add(efUser);
        await dbContext.SaveChangesAsync();

        return mapper.Map<UserDTO>(efUser);
    }

    public async Task<UserDTO> UpdateAsync(UserEditDTO user)
    {
        var existingUser = await dbContext.Users.FindAsync(user.Id) ?? throw new NotFoundException();

        existingUser.Forename = user.Forename;
        existingUser.Surname = user.Surname;

        await dbContext.SaveChangesAsync();

        return mapper.Map<UserDTO>(existingUser);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await dbContext.Users.FindAsync(id) ?? throw new NotFoundException();

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task BorrowAsync(int userId, int bookId)
    {
        var user = await dbContext.Users
            .Include(x => x.Books)
            .SingleOrDefaultAsync(x => x.Id == userId) ?? throw new NotFoundException();

        var book = await dbContext.Books.FindAsync(bookId) ?? throw new NotFoundException();

        if (book.Availability != Availability.Available)
        {
            throw new BookNotAvailableException();
        }

        book.Availability = Availability.CheckedOut;

        var bookList = new List<Book>(user.Books!)
        {
            book
        };
        user.Books = bookList;

        await dbContext.SaveChangesAsync();
    }

    public async Task ReturnAsync(int bookId)
    {
        var book = await dbContext.Books.FindAsync(bookId) ?? throw new NotFoundException();

        if (book.BorrowedById is null && book.Availability == Availability.Available)
        {
            throw new BookNotCheckedOutException();
        }

        book.BorrowedById = null;
        book.Availability = Availability.Available;

        await dbContext.SaveChangesAsync();
    }
}

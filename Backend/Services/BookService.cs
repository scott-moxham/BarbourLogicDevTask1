using AutoMapper;

using Backend.EntityFramework;
using Backend.Exceptions;
using Backend.Models;

using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext dbContext;
    private readonly IMapper mapper;

    public BookService(LibraryDbContext dbContext, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }

    public Task<List<BookDTO>> GetAllAsync()
    {
        return dbContext.Books
            .OrderBy(x => x.Title)
            .Select(x => mapper.Map<BookDTO>(x))
            .ToListAsync();
    }

    public async Task<BookDTOWithUser?> GetByIdAsync(int id)
    {
        var book = await dbContext.Books
            .Include(x => x.BorrowedBy)
            .SingleOrDefaultAsync(x => x.Id == id);

        return book is null
            ? null
            : mapper.Map<BookDTOWithUser>(book);
    }

    public async Task<BookDTO> AddAsync(BookAddDTO book)
    {
        var efBook = mapper.Map<Book>(book);
        efBook.Availability = Availability.Available;

        dbContext.Books.Add(efBook);
        await dbContext.SaveChangesAsync();

        return mapper.Map<BookDTO>(efBook);
    }

    public async Task<BookDTO> UpdateAsync(BookEditDTO book)
    {
        var existingBook = await dbContext.Books.FindAsync(book.Id) ?? throw new NotFoundException();

        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.ISBN = book.ISBN;

        await dbContext.SaveChangesAsync();

        return mapper.Map<BookDTO>(existingBook);
    }

    public async Task DeleteAsync(int id)
    {
        var book = await dbContext.Books.FindAsync(id) ?? throw new NotFoundException();

        dbContext.Books.Remove(book);
        await dbContext.SaveChangesAsync();
    }
}

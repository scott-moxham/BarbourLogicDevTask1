using Backend.Models;

namespace Backend.Services;
public interface IBookService
{
    Task<BookDTO> AddAsync(BookAddDTO book);
    Task DeleteAsync(int id);
    Task<BookDTOWithUser?> GetByIdAsync(int id);
    Task<List<BookDTO>> GetAllAsync();
    Task<BookDTO> UpdateAsync(BookEditDTO book);
}
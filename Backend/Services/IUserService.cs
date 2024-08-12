using Backend.Models;

namespace Backend.Services;
public interface IUserService
{
    Task<UserDTO> AddAsync(UserAddDTO user);
    Task BorrowAsync(int userId, int bookId);
    Task DeleteAsync(int id);
    Task<List<UserDTO>> GetAllAsync();
    Task<UserDTOWithBooks?> GetByIdAsync(int id);
    Task ReturnAsync(int bookId);
    Task<UserDTO> UpdateAsync(UserEditDTO user);
}
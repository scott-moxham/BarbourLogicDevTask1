using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public readonly record struct UserDTO(
    int Id,
    string Forename,
    string Surname
);

public readonly record struct UserAddDTO(
    [param: Required] string Forename,
    [param: Required] string Surname
);

public readonly record struct UserEditDTO(
    [param: Required] int Id,
    [param: Required] string Forename,
    [param: Required] string Surname
);

public readonly record struct UserDTOWithBooks(
    int Id,
    string Forename,
    string Surname,
    IEnumerable<BookDTO> Books
);
using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public readonly record struct BookDTO(
    int Id,
    string Title,
    string Author,
    string ISBN,
    Availability Availability
);

public readonly record struct BookAddDTO(
    [param: Required] string Title,
    [param: Required] string Author,
    [param: Required] string ISBN
);

public readonly record struct BookEditDTO(
    [param: Required] int Id,
    [param: Required] string Title,
    [param: Required] string Author,
    [param: Required] string ISBN
);

public readonly record struct BookDTOWithUser(
    int Id,
    string Title,
    string Author,
    string ISBN,
    Availability Availability,
    UserDTO? BorrowedBy
);
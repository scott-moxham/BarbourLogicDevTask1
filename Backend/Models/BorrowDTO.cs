namespace Backend.Models;

public readonly record struct BorrowDTO(
    int UserId,
    int BookId
);
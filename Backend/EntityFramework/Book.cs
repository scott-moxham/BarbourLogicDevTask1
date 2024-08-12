using Backend.Models;

namespace Backend.EntityFramework;

public class Book
{
    public int? Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string ISBN { get; set; }
    public Availability Availability { get; set; }

    public int? BorrowedById { get; set; }
    public User? BorrowedBy { get; set; }
}

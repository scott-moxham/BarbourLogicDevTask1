namespace Backend.EntityFramework;

public class User
{
    public int? Id { get; set; }
    public required string Forename { get; set; }
    public required string Surname { get; set; }

    public IEnumerable<Book>? Books { get; set; }
}

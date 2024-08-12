namespace Backend.Exceptions;

public class BookNotCheckedOutException : Exception
{
    const string defaultMessage = "The book has not been checked out.";

    public BookNotCheckedOutException() : base(defaultMessage)
    {
    }
}

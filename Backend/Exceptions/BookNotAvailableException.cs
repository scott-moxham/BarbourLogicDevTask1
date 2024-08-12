namespace Backend.Exceptions;

public class BookNotAvailableException : Exception
{
    const string defaultMessage = "The book is not available for borrowing.";

    public BookNotAvailableException() : base(defaultMessage)
    {
    }
}

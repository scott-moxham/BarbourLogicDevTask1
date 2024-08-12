namespace Backend.Exceptions;

public class NotFoundException : Exception
{
    const string defaultMessage = "The requested resource was not found.";

    public NotFoundException() : base(defaultMessage)
    {
    }
}

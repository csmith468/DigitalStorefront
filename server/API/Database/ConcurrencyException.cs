namespace API.Database;

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}
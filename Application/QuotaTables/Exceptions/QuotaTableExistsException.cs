namespace Application.QuotaTables.Exceptions;

public class QuotaTableExistsException : Exception
{
    public QuotaTableExistsException() { }
    public QuotaTableExistsException(string message) : base(message) { }
    public QuotaTableExistsException(string message, Exception innerException) : base(message, innerException) { }
}
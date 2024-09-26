namespace Application.QuotaTables.Exceptions;

public class GenericQuotaException : Exception
{
    public GenericQuotaException() { }
    public GenericQuotaException(string message) : base(message) { }
    public GenericQuotaException(string message, Exception innerException) : base(message, innerException) { }
}
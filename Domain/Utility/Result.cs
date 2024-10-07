namespace Domain.Utility;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string ErrorMessage { get; }

    protected Result(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, string.Empty);

    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    public static Result Failure(string message) => new(false, message);

    public static Result<T> Failure<T>(string message) => new(default, false, message);
}

public class Result<T> : Result
{
    private readonly T _value;

    public T Value
    {
        get
        {
            if (!IsSuccess)
            {
                throw new InvalidOperationException();
            }

            return _value;
        }
    }

    protected internal Result(T value, bool isSuccess, string errorMessage) : base(isSuccess, errorMessage)
    {
        _value = value;
    }
}
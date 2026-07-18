namespace Storefront.SharedKernel;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, default!);
    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<T>
{
    private readonly T _value;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access Value on a failure result.");
    public Error Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        Error = default!;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default!;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}


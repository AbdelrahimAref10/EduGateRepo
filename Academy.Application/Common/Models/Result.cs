namespace Academy.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public int StatusCode { get; }

    protected Result(bool isSuccess, string error, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success() => new(true, string.Empty, 200);

    public static Result Failure(string error, int statusCode = 400) =>
        new(false, error, statusCode);

    public static Result NotFound(string error) =>
        new(false, error, 404);

    public static Result Conflict(string error) =>
        new(false, error, 409);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, string.Empty, 200) => Value = value;

    private Result(string error, int statusCode) : base(false, error, statusCode) { }

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(string error, int statusCode = 400) =>
        new(error, statusCode);

    public static new Result<T> NotFound(string error) =>
        new(error, 404);

    public static new Result<T> Conflict(string error) =>
        new(error, 409);
}

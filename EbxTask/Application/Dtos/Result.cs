namespace EbxTask.Application.Models;

public class Result<T>
{
    public T Value { get; private set; }
    public string Error { get; private set; }
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;

    protected Result(T value, bool isSuccess, string error = null)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<T> Success(T value) => new Result<T>(value, true);
    public static Result<T> Failure(string error) => new Result<T>(default, false, error);
}
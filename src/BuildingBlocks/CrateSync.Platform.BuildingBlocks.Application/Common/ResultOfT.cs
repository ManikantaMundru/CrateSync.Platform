using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.BuildingBlocks.Application.Common;

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(
        T value)
        : base(
            true,
            Error.None)
    {
        _value = value;
    }

    private Result(
        Error error)
        : base(
            false,
            error)
    {
        _value = default;
    }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "The value of a failed result cannot be accessed.");

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static new Result<T> Failure(Error error)
    {
        return new Result<T>(error);
    }

    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }
}

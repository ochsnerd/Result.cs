using System.Diagnostics.CodeAnalysis;

namespace Result;

public record Error(string Message);

public record Result<T>
{
    public bool IsOk => isOk;
    public bool IsError => isOk is false;

    public T Value => IsOk ? value! : throw new InvalidOperationException("Result is an error, can't get Value");
    public Error Error => IsError ? error! : throw new InvalidOperationException("Result is ok, can't get Error");

    public static Result<T> FromOk(T value) => new(true, value, null);
    public static Result<T> FromError(Error error) => new(false, default, error);
    public static implicit operator Result<T>(T value) => FromOk(value);
    public static implicit operator Result<T>(Error error) => FromError(error);

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = IsOk ? Value : default;
        return IsOk;
    }

    public bool TryGetError([MaybeNullWhen(false)] out Error error)
    {
        error = IsOk ? null : Error;
        return IsOk is false;
    }

    public Result<T> MapError(Func<Error, Error> function)
    {
        return IsError ? function(Error) : this;
    }

    public Result<T> AndThenError(Func<Error, Result<T>> function)
    {
        return IsError ? function(Error) : this;
    }

    public Result<T> OrElse(Func<Error, Result<T>> function)
    {
        return IsError ? function(Error) : this;
    }

    public Result<T> OrElse(T value)
    {
        return IsError ? value : this;
    }

    public override string ToString()
    {
        return IsOk ? $"Result.Ok({Value})" : $"Result.Error({Error.Message})";
    }

    private readonly bool isOk;
    private readonly T? value;
    private readonly Error? error;

    private Result(bool isOk, T? value, Error? error)
    {
        this.isOk = isOk;
        this.value = value;
        this.error = error;
    }
}

public static class ResultExtensions
{
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> f)
    {
        return result.IsOk ? f(result.Value) : result.Error;
    }

    public static Result<U> AndThen<T, U>(this Result<T> result, Func<T, Result<U>> g)
    {
        return result.IsOk ? g(result.Value) : result.Error;
    }

    public static async Task<Result<U>> MapAsync<T, U>(this Result<T> result, Func<T, Task<U>> f)
    {
        return result.IsOk ? await f(result.Value) : result.Error;
    }

    public static async Task<Result<U>> AndThenAsync<T, U>(this Result<T> result, Func<T, Task<Result<U>>> g)
    {
        return result.IsOk ? await g(result.Value) : result.Error;
    }
}
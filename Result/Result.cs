using System.Diagnostics;
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

public static class TryGets
{
    public static bool TryGetValue<T>(this Result<T> result, [MaybeNullWhen(false)] out T value)
    {
        value = result.IsOk ? result.Value : default;
        return result.IsOk;
    }

    public static bool TryGetError<T>(this Result<T> result, [MaybeNullWhen(false)] out Error error)
    {
        error = result.IsOk ? null : result.Error;
        return result.IsError;
    }
}

public static class MonadicOperations
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

    // See for example https://fsharpforfunandprofit.com/posts/elevated-world/#lift
    // but we're missing some language features to do it as elegantly (partial application).
// These have the same semantics as "lifted operators for nullable value types":
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types#lifted-operators
public static class ApplicativeOperations
{
    public static Result<U> Lifted<T, U>(this Func<T, U> f, Result<T> tr)
    {
        return tr.Map(f);
    }

    public static Result<V> Lifted<T, U, V>(this Func<T, U, V> f, Result<T> tr, Result<U> ur)
    {
        if (tr.TryGetValue(out var t) && ur.TryGetValue(out var u))
        {
            return f(t, u);
        }
        if (tr.TryGetError(out var e) || ur.TryGetError(out e))
        {
            return e;
        }
        throw new UnreachableException();
    }

    public static Result<W> Lifted<T, U, V, W>(this Func<T, U, V, W> f, Result<T> tr, Result<U> ur, Result<V> vr)
    {
        if (tr.TryGetValue(out var t) && ur.TryGetValue(out var u) && vr.TryGetValue(out var v))
        {
            return f(t, u, v);
        }
        if (tr.TryGetError(out var e) || ur.TryGetError(out e) || vr.TryGetError(out e))
        {
            return e;
        }
        throw new UnreachableException();
    }

    // and so on and so forth, C# goes up to 16 Arguments: https://learn.microsoft.com/en-us/dotnet/api/system.func-17?view=net-9.0
}


public static class ErrorOperations
{
    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> function)
    {
        return result.IsError ? function(result.Error) : result;
    }

    public static Result<T> AndThenError<T>(this Result<T> result, Func<Error, Result<T>> function)
    {
        return result.IsError ? function(result.Error) : result;
    }

    public static Result<T> OrElse<T>(this Result<T> result, Func<Error, Result<T>> function)
    {
        return result.IsError ? function(result.Error) : result;
    }

    public static Result<T> OrElse<T>(this Result<T> result, T value)
    {
        return result.IsError ? value : result;
    }
}

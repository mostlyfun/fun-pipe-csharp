using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
namespace Fun;

/// <summary>
/// Static extension or utility methods for Opt, Res, or Pipe.
/// </summary>
public static class Extensions
{
    // Opt
    /// <summary>
    /// Returns Some of <paramref name="value"/> only if it is not null; None otherwise.
    /// </summary>
    public static Opt<T> Some<T>(T value)
    {
        if (typeof(T).IsClass)
            return value == null ? Opt<T>.None : Opt<T>.Some(value);
        else
            return Opt<T>.Some(value);
    }
    /// <summary>
    /// Returns back <paramref name="maybeValue"/>.
    /// </summary>
    public static Opt<T> Some<T>(Opt<T> maybeValue) => maybeValue;
    /// <summary>
    /// Returns Some of result.Unwrap() only if it IsOk; None otherwise.
    /// </summary>
    public static Opt<T> Some<T>(Res<T> result)
        => result.IsOk ? Opt<T>.Some(result.Unwrap()) : Opt<T>.None;
    /// <summary>
    /// <inheritdoc cref="Opt{T}.None"/>
    /// </summary>
    public static Opt<T> None<T>() => Opt<T>.None;


    // Opt - Comparisons
    /// <summary>
    /// Returns minimum of the two option values <paramref name="a"/> and <paramref name="b"/>; None's are ignored in comparison while min of two None's is again None.
    /// </summary>
    public static Opt<T> Min<T>(Opt<T> a, Opt<T> b) where T : IComparable
    {
        if (a.IsNone)
            return b;
        if (b.IsNone || a.value.CompareTo(b.value) < 0)
            return a;
        return b;
    }
    /// <summary>
    /// Returns maximum of the two option values <paramref name="a"/> and <paramref name="b"/>; None's are ignored in comparison while max of two None's is again None.
    /// </summary>
    public static Opt<T> Max<T>(Opt<T> a, Opt<T> b) where T : IComparable
    {
        if (a.IsNone)
            return b;
        if (b.IsNone || a.value.CompareTo(b.value) > 0)
            return a;
        return b;
    }


    // Opt - Collection
    /// <summary>
    /// Creates and returns and IEnumerable yielding unwrapped values of elements that are <see cref="Opt{T}.IsSome"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of the Opt.</typeparam>
    /// <param name="collection">Collection of opt values.</param>
    public static IEnumerable<T> UnwrapValues<T>(this IEnumerable<Opt<T>> collection)
        => collection.Where(x => x.IsSome).Select(x => x.Unwrap());
    /// <summary>
    /// Opt counterpart of FirstOrDefault, which returns the first non-null element if <paramref name="collection"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> collection)
    {
        if (typeof(T).IsClass)
        {
            foreach (var item in collection)
                if (item != null)
                    return Opt<T>.Some(item);
            return Opt<T>.None;
        }
        else
        {
            foreach (var item in collection)
                return Opt<T>.Some(item);
            return Opt<T>.None;
        }
    }
    /// <summary>
    /// Opt counterpart of FirstOrDefault, which returns the first Some element of <paramref name="collection"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<Opt<T>> collection)
    {
        foreach (var item in collection)
            if (item.IsSome)
                return item;
        return Opt<T>.None;
    }


    // Res
    /// <summary>
    /// <inheritdoc cref="Res.Ok"/>
    /// </summary>
    public static Res Ok() => Res.Ok;
    /// <summary>
    /// <inheritdoc cref="Res.Err(string)"/>
    /// </summary>
    public static Res Err(string message = "") => Res.Err(message);
    /// <summary>
    /// <inheritdoc cref="Res.Err(Exception)"/>
    /// </summary>
    public static Res Err(Exception exception) => Res.Err(exception);
    /// <summary>
    /// <inheritdoc cref="Res.Err(string, Exception)"/>
    /// </summary>
    public static Res Err(string message, Exception exception) => Res.Err(message, exception);


    // ResT
    /// <summary>
    /// <inheritdoc cref="Res{T}.Ok(T)"/>
    /// </summary>
    public static Res<T> Ok<T>(T value)
    {
        if (typeof(T).IsClass)
            return value == null ? Res<T>.Err("null") : Res<T>.Ok(value);
        else
            return Res<T>.Ok(value);
    }
    /// <summary>
    /// Returns Ok result with the given <paramref name="responseMessage"/> only if its <see cref="HttpStatusCode"/> is OK; Err otherwise.
    /// </summary>
    public static Res<HttpResponseMessage> Ok(HttpResponseMessage responseMessage)
        => responseMessage.StatusCode == HttpStatusCode.OK ? Res<HttpResponseMessage>.Ok(responseMessage) : Res<HttpResponseMessage>.Err($"statusCode:{responseMessage.StatusCode}");
    /// <summary>
    /// Returns Ok result with the given <paramref name="maybeValue"/> only if it is Some; Err otherwise.
    /// </summary>
    public static Res<T> Ok<T>(Opt<T> maybeValue)
        => maybeValue.IsSome ? Res<T>.Ok(maybeValue.Unwrap()) : Res<T>.Err("None");
    /// <summary>
    /// Returns back <paramref name="result"/>.
    /// </summary>
    public static Res<T> Ok<T>(Res<T> result) => result;
    /// <summary>
    /// <inheritdoc cref="Res.Err(string)"/>
    /// </summary>
    public static Res<T> Err<T>(string message = "") => Res<T>.Err(message);
    /// <summary>
    /// <inheritdoc cref="Res.Err(Exception)"/>
    /// </summary>
    public static Res<T> Err<T>(Exception exception) => Res<T>.Err(exception);
    /// <summary>
    /// <inheritdoc cref="Res.Err(string, Exception)"/>
    /// </summary>
    public static Res<T> Err<T>(string message, Exception exception) => Res<T>.Err(message, exception);


    // ResT - Collection
    /// <summary>
    /// Creates and returns and IEnumerable yielding unwrapped values of elements that are <see cref="Res{T}.IsOk"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of the Res.</typeparam>
    /// <param name="collection">Collection of result values.</param>
    public static IEnumerable<T> UnwrapValues<T>(this IEnumerable<Res<T>> collection)
        => collection.Where(x => x.IsOk).Select(x => x.Unwrap());
    /// <summary>
    /// Opt counterpart of FirstOrDefault over results collection, which returns the first Ok element of <paramref name="collection"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<Res<T>> collection)
    {
        foreach (var item in collection)
            if (item.IsOk)
                return Some(item);
        return Opt<T>.None;
    }


    // Pipe
    /// <summary>
    /// <inheritdoc cref="Pipe.New(OnErr)"/>
    /// </summary>
    public static Pipe NewPipe(OnErr onErr = OnErr.None) => Pipe.New(onErr);
}

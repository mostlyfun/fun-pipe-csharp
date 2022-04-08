using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace Fun;

/// <summary>
/// Static extension or utility methods for Opt, Res, or Pipe.
/// </summary>
public static partial class Extensions
{
    // Opt - None
    /// <summary>
    /// Maps <paramref name="maybe"/> into <paramref name="some"/>(maybe.Unwrap()) whenever maybe.IsSome; and into <paramref name="none"/>() otherwise.
    /// </summary>
    public static TOut Match<T, TOut>(this Opt<T> maybe, Func<T, TOut> some, Func<TOut> none)
    {
        if (maybe.IsSome) return some(maybe.value);
        else return none();
    }
    /// <summary>
    /// Maps <paramref name="maybe"/> into <paramref name="some"/>(maybe.Unwrap()) whenever maybe.IsSome; and into <paramref name="none"/> otherwise.
    /// </summary>
    public static TOut Match<T, TOut>(this Opt<T> maybe, Func<T, TOut> some, TOut none)
    {
        if (maybe.IsSome) return some(maybe.value);
        else return none;
    }


    // Run
    /// <summary>
    /// Runs <paramref name="action"/>, and returns back <paramref name="value"/>.
    /// </summary>
    public static T Run<T>(this T value, Action action) { action(); return value; }
    /// <summary>
    /// Runs <paramref name="action"/>(<paramref name="value"/>), and returns back <paramref name="value"/>.
    /// </summary>
    public static T Run<T>(this T value, Action<T> action) { action(value); return value; }


    // Map: ()->T
    /// <summary>
    /// Returns <paramref name="map"/>().
    /// </summary>
    public static TOut Map<TOut>(Func<TOut> map) => map();

    // Map: t->T
    /// <summary>
    /// Returns <paramref name="map"/>().
    /// </summary>
    public static TOut Map<T, TOut>(this T value, Func<TOut> map) => map();
    /// <summary>
    /// Returns <paramref name="map"/>(<paramref name="value"/>).
    /// </summary>
    public static TOut Map<T, TOut>(this T value, Func<T, TOut> map) => map(value);


    // Collections
    /// <summary>
    /// Creates and returns and IEnumerable yielding unwrapped values of elements that are <see cref="Opt{T}.IsSome"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of the Opt.</typeparam>
    /// <param name="collection">Collection of opt values.</param>
    public static IEnumerable<T> UnwrapValues<T>(this IEnumerable<Opt<T>> collection) => collection.Where(x => x.IsSome).Select(x => x.Unwrap());
    /// <summary>
    /// Creates and returns and IEnumerable yielding unwrapped values of elements that are <see cref="Res{T}.IsOk"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of the Res.</typeparam>
    /// <param name="collection">Collection of result values.</param>
    public static IEnumerable<T> UnwrapValues<T>(this IEnumerable<Res<T>> collection) => collection.Where(x => x.IsOk).Select(x => x.Unwrap());
}

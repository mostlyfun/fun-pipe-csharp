namespace Fun;

/// <summary>
/// Static extension or utility methods for Opt, Res, or Pipe.
/// </summary>
public static partial class Extensions
{
    // Unwrap
    /// <summary>
    /// Returns IEnumerable yielding unwrapped values of elements that are <see cref="Opt{T}.IsSome"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of the Opt.</typeparam>
    /// <param name="collection">Collection of opt values.</param>
    public static IEnumerable<T> UnwrapValues<T>(this IEnumerable<Opt<T>> collection)
        => collection.Where(x => x.IsSome).Select(x => x.Unwrap());
    /// <summary>
    /// Returns IEnumerable yielding unwrapped values of elements that are <see cref="Res{T}.IsOk"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of the Res.</typeparam>
    /// <param name="collection">Collection of result values.</param>
    public static IEnumerable<T> UnwrapValues<T>(this IEnumerable<Res<T>> collection)
        => collection.Where(x => x.IsOk).Select(x => x.Unwrap());


    // First
    /// <summary>
    /// Opt counterpart of FirstOrDefault, which returns the first non-null element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> enumerable)
        => Some(enumerable.FirstOrDefault(x => x != null));
    /// <summary>
    /// Opt counterpart of FirstOrDefault, which returns the first non-null element if <paramref name="enumerable"/> has any satisfying the <paramref name="predicate"/>, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        => Some(enumerable.FirstOrDefault(x => x != null && predicate(x)));
    // FirstSome
    /// <summary>
    /// Returns first IsSome element of the <paramref name="enumerable"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> FirstSomeOrNone<T>(this IEnumerable<Opt<T>> enumerable)
        => enumerable.FirstOrDefault(x => x.IsSome);
    /// <summary>
    /// Returns first IsSome element of the <paramref name="enumerable"/> whose unwrapped value satisfies the <paramref name="predicate"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> FirstSomeOrNone<T>(this IEnumerable<Opt<T>> enumerable, Func<T, bool> predicate)
        => enumerable.FirstOrDefault(x => x.IsSome && predicate(x.Unwrap()));
    // FirstOk
    /// <summary>
    /// Returns first IsOk element of the <paramref name="enumerable"/> if any, Err otherwise.
    /// </summary>
    public static Opt<T> FirstOkOrNone<T>(this IEnumerable<Res<T>> enumerable)
        => enumerable.FirstOrDefault(x => x.IsOk).ToOpt();
    /// <summary>
    /// Returns first IsOk element of the <paramref name="enumerable"/> whose unwrapped value satisfies the <paramref name="predicate"/> if any, Err otherwise.
    /// </summary>
    public static Opt<T> FirstOkOrNone<T>(this IEnumerable<Res<T>> enumerable, Func<T, bool> predicate)
        => enumerable.FirstOrDefault(x => x.IsOk && predicate(x.Unwrap())).ToOpt();


    // Last
    /// <summary>
    /// Opt counterpart of LastOrDefault, which returns the last non-null element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<T> enumerable)
        => Some(enumerable.LastOrDefault(x => x != null));
    /// <summary>
    /// Opt counterpart of LastOrDefault, which returns the last non-null element if <paramref name="enumerable"/> has any satisfying the <paramref name="predicate"/>, None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        => Some(enumerable.LastOrDefault(x => x != null && predicate(x)));
    // LastSome
    /// <summary>
    /// Returns last IsSome element of the <paramref name="enumerable"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> LastSomeOrNone<T>(this IEnumerable<Opt<T>> enumerable)
        => enumerable.LastOrDefault(x => x.IsSome);
    /// <summary>
    /// Returns last IsSome element of the <paramref name="enumerable"/> whose unwrapped value satisfies the <paramref name="predicate"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> LastSomeOrNone<T>(this IEnumerable<Opt<T>> enumerable, Func<T, bool> predicate)
        => enumerable.LastOrDefault(x => x.IsSome && predicate(x.Unwrap()));
    // LastOk
    /// <summary>
    /// Returns last IsOk element of the <paramref name="enumerable"/> if any, Err otherwise.
    /// </summary>
    public static Opt<T> LastOkOrNone<T>(this IEnumerable<Res<T>> enumerable)
        => enumerable.LastOrDefault(x => x.IsOk).ToOpt();
    /// <summary>
    /// Returns last IsOk element of the <paramref name="enumerable"/> whose unwrapped value satisfies the <paramref name="predicate"/> if any, Err otherwise.
    /// </summary>
    public static Opt<T> LastOkOrNone<T>(this IEnumerable<Res<T>> enumerable, Func<T, bool> predicate)
        => enumerable.LastOrDefault(x => x.IsOk && predicate(x.Unwrap())).ToOpt();
}

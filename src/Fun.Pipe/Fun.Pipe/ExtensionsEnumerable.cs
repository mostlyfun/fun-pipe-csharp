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
        => enumerable.FirstOrDefault(x => x.IsOk).AsOpt();
    /// <summary>
    /// Returns first IsOk element of the <paramref name="enumerable"/> whose unwrapped value satisfies the <paramref name="predicate"/> if any, Err otherwise.
    /// </summary>
    public static Opt<T> FirstOkOrNone<T>(this IEnumerable<Res<T>> enumerable, Func<T, bool> predicate)
        => enumerable.FirstOrDefault(x => x.IsOk && predicate(x.Unwrap())).AsOpt();


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
        => enumerable.LastOrDefault(x => x.IsOk).AsOpt();
    /// <summary>
    /// Returns last IsOk element of the <paramref name="enumerable"/> whose unwrapped value satisfies the <paramref name="predicate"/> if any, Err otherwise.
    /// </summary>
    public static Opt<T> LastOkOrNone<T>(this IEnumerable<Res<T>> enumerable, Func<T, bool> predicate)
        => enumerable.LastOrDefault(x => x.IsOk && predicate(x.Unwrap())).AsOpt();


    // AnySome
    /// <summary>
    /// Returns whether <paramref name="enumerable"/> has any IsSome element or not.
    /// </summary>
    public static bool AnySome<T>(this IEnumerable<Opt<T>> enumerable)
        => FirstSomeOrNone(enumerable).IsSome;
    /// <summary>
    /// Returns whether <paramref name="enumerable"/> has any IsSome element whose value satisfies the <paramref name="predicate"/> or not.
    /// </summary>
    public static bool AnySome<T>(this IEnumerable<Opt<T>> enumerable, Func<T, bool> predicate)
        => FirstSomeOrNone(enumerable, predicate).IsSome;
    // AnyOk
    /// <summary>
    /// Returns whether <paramref name="enumerable"/> has any IsSome element or not.
    /// </summary>
    public static bool AnyOk<T>(this IEnumerable<Res<T>> enumerable)
        => FirstOkOrNone(enumerable).IsSome;
    /// <summary>
    /// Returns whether <paramref name="enumerable"/> has any IsSome element whose value satisfies the <paramref name="predicate"/> or not.
    /// </summary>
    public static bool AnyOk<T>(this IEnumerable<Res<T>> enumerable, Func<T, bool> predicate)
        => FirstOkOrNone(enumerable, predicate).IsSome;

    
    // AllSome
    /// <summary>
    /// Returns whether all elements of <paramref name="enumerable"/> are IsSome or not.
    /// </summary>
    public static bool AllSome<T>(this IEnumerable<Opt<T>> enumerable)
        => enumerable.All(x => x.IsSome);
    /// <summary>
    /// Returns whether all elements of <paramref name="enumerable"/> are IsSome unwrapped value of which satisfies the <paramref name="predicate"/> or not.
    /// </summary>
    public static bool AllSome<T>(this IEnumerable<Opt<T>> enumerable, Func<T, bool> predicate)
        => enumerable.All(x => x.IsSome && predicate(x.Unwrap()));
    // AllOk
    /// <summary>
    /// Returns whether all elements of <paramref name="enumerable"/> are IsOk or not.
    /// </summary>
    public static bool AllOk<T>(this IEnumerable<Res<T>> enumerable)
        => enumerable.All(x => x.IsOk);
    /// <summary>
    /// Returns whether all elements of <paramref name="enumerable"/> are IsOk unwrapped value of which satisfies the <paramref name="predicate"/> or not.
    /// </summary>
    public static bool AllOk<T>(this IEnumerable<Res<T>> enumerable, Func<T, bool> predicate)
        => enumerable.All(x => x.IsOk && predicate(x.Unwrap()));


    // SelectSome
    /// <summary>
    /// Maps unwrapped values of IsSome elements of the <paramref name="enumerable"/> by the <paramref name="selector"/>.
    /// </summary>
    public static IEnumerable<TOut> SelectSome<T, TOut>(this IEnumerable<Opt<T>> enumerable, Func<T, TOut> selector)
        => enumerable.Where(x => x.IsSome).Select(x => selector(x.Unwrap()));
    // SelectOk
    /// <summary>
    /// Maps unwrapped values of IsOk elements of the <paramref name="enumerable"/> by the <paramref name="selector"/>.
    /// </summary>
    public static IEnumerable<TOut> SelectOk<T, TOut>(this IEnumerable<Res<T>> enumerable, Func<T, TOut> selector)
        => enumerable.Where(x => x.IsOk).Select(x => selector(x.Unwrap()));


    // WhereSome
    /// <summary>
    /// Filters unwrapped values of IsSome elements of the <paramref name="enumerable"/> by the <paramref name="predicate"/>.
    /// </summary>
    public static IEnumerable<T> WhereSome<T>(this IEnumerable<Opt<T>> enumerable, Func<T, bool> predicate)
        => enumerable.Where(x => x.IsSome && predicate(x.Unwrap())).Select(x => x.Unwrap());
    // WhereOk
    /// <summary>
    /// Filters unwrapped values of IsOk elements of the <paramref name="enumerable"/> by the <paramref name="predicate"/>.
    /// </summary>
    public static IEnumerable<T> WhereOk<T>(this IEnumerable<Res<T>> enumerable, Func<T, bool> predicate)
        => enumerable.Where(x => x.IsOk && predicate(x.Unwrap())).Select(x => x.Unwrap());


    // ForEachSome
    /// <summary>
    /// Applies the <paramref name="action"/> on unwrapped value of each IsSome elements of the <paramref name="enumerable"/>.
    /// </summary>
    public static void ForEachSome<T>(this IEnumerable<Opt<T>> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            if (item.IsSome)
                action(item.Unwrap());
    }
    /// <summary>
    /// Applies the <paramref name="action"/> on unwrapped value of each IsSome elements of the <paramref name="enumerable"/> that satisfies the <paramref name="predicate"/>.
    /// </summary>
    public static void ForEachSome<T>(this IEnumerable<Opt<T>> enumerable, Func<T, bool> predicate, Action<T> action)
    {
        foreach (var item in enumerable)
            if (item.IsSome && predicate(item.Unwrap()))
                action(item.Unwrap());
    }
    // ForEachOk
    /// <summary>
    /// Applies the <paramref name="action"/> on unwrapped value of each IsOk elements of the <paramref name="enumerable"/>.
    /// </summary>
    public static void ForEachOk<T>(this IEnumerable<Res<T>> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            if (item.IsOk)
                action(item.Unwrap());
    }
    /// <summary>
    /// Applies the <paramref name="action"/> on unwrapped value of each IsOk elements of the <paramref name="enumerable"/> that satisfies the <paramref name="predicate"/>.
    /// </summary>
    public static void ForEachOk<T>(this IEnumerable<Res<T>> enumerable, Func<T, bool> predicate, Action<T> action)
    {
        foreach (var item in enumerable)
            if (item.IsOk && predicate(item.Unwrap()))
                action(item.Unwrap());
    }
}

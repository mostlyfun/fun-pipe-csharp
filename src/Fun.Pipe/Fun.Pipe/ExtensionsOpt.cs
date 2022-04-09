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
    // Constants
    const string errNone = "None->Res";


    // New
    /// <summary>
    /// Creates Some of <typeparamref name="T"/> with the given <paramref name="value"/>.
    /// Note that 'null' is not allowed and automatically mapped to None.
    /// </summary>
    public static Opt<T> Some<T>(T value)
        => new(value);
    /// <summary>
    /// Creates None of <typeparamref name="T"/>.
    /// </summary>
    public static Opt<T> None<T>()
        => new();
    // ToOpt
    /// <summary>
    /// Converts Res to Opt: maps <paramref name="result"/> to Some of its value when IsOk; to None when IsErr.
    /// </summary>
    public static Opt<T> ToOpt<T>(this Res<T> result)
        => result.IsErr ? None<T>() : Some(result.value);
    /// <summary>
    /// Converts <paramref name="list"/> of <typeparamref name="T"/> into a list of Opt&lt;<typeparamref name="T"/>>.
    /// If <typeparamref name="T"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static IEnumerable<Opt<T>> ToOpt<T>(this IEnumerable<T> list)
        => list.Select(x => Some(x));
    /// <summary>
    /// Converts <paramref name="dictionary"/> of <typeparamref name="TKey"/>-<typeparamref name="TValue"/> pair into a dictionary with of Opt&lt;<typeparamref name="TValue"/>> as the value type.
    /// If <typeparamref name="TValue"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static Dictionary<TKey, Opt<TValue>> ToOpt<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        var opt = new Dictionary<TKey, Opt<TValue>>(dictionary.Count);
        foreach (var item in dictionary)
            opt.Add(item.Key, Some(item.Value));
        return opt;
    }


    // Logical
    /// <summary>
    /// Returns back <paramref name="first"/> (this) if it is Some; returns <paramref name="second"/> otherwise.
    /// </summary>
    public static Opt<T> Or<T>(this Opt<T> first, Opt<T> second)
        => first.IsSome ? first : second;


    // Opt - None
    /// <summary>
    /// Does nothing and returns itself when <paramref name="maybe"/> IsSome; throws when IsNone.
    /// </summary>
    public static Opt<T> ThrowIfNone<T>(this Opt<T> maybe)
    { if (maybe.IsNone) throw new ArgumentException("[err] None"); return maybe; }
    /// <summary>
    /// Does nothing and returns itself when <paramref name="maybe"/> IsSome; throws with the given <paramref name="errorMessage"/> when IsNone.
    /// </summary>
    public static Opt<T> ThrowIfNone<T>(this Opt<T> maybe, string errorMessage)
    { if (maybe.IsNone) throw new ArgumentException($"[err] None: {errorMessage}"); return maybe; }
    /// <summary>
    /// Does nothing when <paramref name="maybe"/> IsSome; logs the given <paramref name="errorMessage"/> when IsNone.
    /// Returns itself.
    /// </summary>
    public static Opt<T> LogIfNone<T>(this Opt<T> maybe, string errorMessage)
    { if (maybe.IsNone) Console.WriteLine($"[warn] None: {errorMessage}"); return maybe; }
    /// <summary>
    /// Does nothing when <paramref name="maybe"/> IsSome; runs the given <paramref name="action"/> when IsNone.
    /// Returns itself.
    /// </summary>
    public static Opt<T> RunIfNone<T>(this Opt<T> maybe, Action action)
    { if (maybe.IsNone) action(); return maybe; }
    

    // Run
    /// <summary>
    /// Runs <paramref name="action"/>() only if maybe.IsSome, and returns back <paramref name="maybe"/>.
    /// </summary>
    public static Opt<T> Run<T>(this Opt<T> maybe, Action action)
    { if (maybe.IsSome) action(); return maybe; }
    /// <summary>
    /// Runs <paramref name="action"/>(<paramref name="maybe"/>.Unwrap()) only if maybe.IsSome, and returns back <paramref name="maybe"/>.
    /// </summary>
    public static Opt<T> Run<T>(this Opt<T> maybe, Action<T> action)
    { if (maybe.IsSome) action(maybe.value); return maybe; }


    // Map: Opt<t>->Opt<T>
    /// <summary>
    /// Returns None when <paramref name="maybe"/> IsNone; Some(<paramref name="map"/>()) when IsSome.
    /// </summary>
    public static Opt<TOut> Map<T, TOut>(this Opt<T> maybe, Func<TOut> map)
        => maybe.IsNone ? None<TOut>() : Some(map());
    /// <summary>
    /// Returns None when <paramref name="maybe"/> IsNone; Some(<paramref name="map"/>(maybe.Unwrap())) when IsSome.
    /// </summary>
    public static Opt<TOut> Map<T, TOut>(this Opt<T> maybe, Func<T, TOut> map)
        => maybe.IsNone ? None<TOut>() : Some(map(maybe.value));
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{TOut})"/>
    /// </summary>
    public static Opt<TOut> Map<T, TOut>(this Opt<T> maybe, Func<Opt<TOut>> map)
        => maybe.IsNone ? None<TOut>() : map();
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{T, TOut})"/>
    /// </summary>
    public static Opt<TOut> Map<T, TOut>(this Opt<T> maybe, Func<T, Opt<TOut>> map)
        => maybe.IsNone ? None<TOut>() : map(maybe.value);


    #region ASYNC

    // Run
    /// <summary>
    /// <inheritdoc cref="Run{T}(T, Action)"/>
    /// </summary>
    public static Task<T> RunAsync<T>(this T value, Func<Task> action)
    { action(); return Task.FromResult(value); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(T, Action{T})"/>
    /// </summary>
    public static Task<T> RunAsync<T>(this T value, Func<T, Task> action)
    { action(value); return Task.FromResult(value); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(Opt{T}, Action)"/>
    /// </summary>
    public static Task<Opt<T>> RunAsync<T>(this Opt<T> maybe, Func<Task> action)
    { if (maybe.IsSome) action(); return Task.FromResult(maybe); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(Opt{T}, Action{T})"/>
    /// </summary>
    public static Task<Opt<T>> RunAsync<T>(this Opt<T> maybe, Func<T, Task> action)
    { if (maybe.IsSome) action(maybe.value); return Task.FromResult(maybe); }


    // Map: ()->T
    /// <summary>
    /// <inheritdoc cref="Map{TOut}(Func{TOut})"/>
    /// </summary>
    public static Task<TOut> MapAsync<TOut>(Func<Task<TOut>> map) 
        => map();


    // Map: t->T
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(T, Func{T, TOut})"/>
    /// </summary>
    public static Task<TOut> MapAsync<T, TOut>(this T value, Func<T, Task<TOut>> map)
        => map(value);


    // Map: Opt<t>->Opt<T>
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{TOut})"/>
    /// </summary>
    public static async Task<Opt<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<Task<TOut>> map)
        => maybe.IsNone ? None<TOut>() : Some(await map());
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{T, TOut})"/>
    /// </summary>
    public static async Task<Opt<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<TOut>> map)
        => maybe.IsNone ? None<TOut>() : Some(await map(maybe.value));
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{TOut})"/>
    /// </summary>
    public static Task<Opt<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<Task<Opt<TOut>>> map)
        => maybe.IsNone ? Task.FromResult(None<TOut>()) : map();
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{T, TOut})"/>
    /// </summary>
    public static Task<Opt<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<Opt<TOut>>> map)
        => maybe.IsNone ? Task.FromResult(None<TOut>()) : map(maybe.value);
  
    #endregion


    // TryParse
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<int> ParseIntOrNone(this string text)
    { bool s = int.TryParse(text, out var val); return s ? Some(val) : None<int>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<int> ParseIntOrNone(this ReadOnlySpan<char> text)
    { bool s = int.TryParse(text, out var val); return s ? Some(val) : None<int>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<double> ParseDoubleOrNone(this string text)
    { bool s = double.TryParse(text, out var val); return s ? Some(val) : None<double>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<double> ParseDoubleOrNone(this ReadOnlySpan<char> text)
    { bool s = double.TryParse(text, out var val); return s ? Some(val) : None<double>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<float> ParseFloatOrNone(this string text)
    { bool s = float.TryParse(text, out var val); return s ? Some(val) : None<float>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<float> ParseFloatOrNone(this ReadOnlySpan<char> text)
    { bool s = float.TryParse(text, out var val); return s ? Some(val) : None<float>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<short> ParseShortOrNone(this string text)
    { bool s = short.TryParse(text, out var val); return s ? Some(val) : None<short>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<short> ParseShortOrNone(this ReadOnlySpan<char> text)
    { bool s = short.TryParse(text, out var val); return s ? Some(val) : None<short>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<long> ParseLongOrNone(this string text)
    { bool s = long.TryParse(text, out var val); return s ? Some(val) : None<long>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<long> ParseLongOrNone(this ReadOnlySpan<char> text)
    { bool s = long.TryParse(text, out var val); return s ? Some(val) : None<long>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<Half> ParseHalfOrNone(this string text)
    { bool s = Half.TryParse(text, out var val); return s ? Some(val) : None<Half>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<Half> ParseHalfOrNone(this ReadOnlySpan<char> text)
    { bool s = Half.TryParse(text, out var val); return s ? Some(val) : None<Half>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<bool> ParseBoolOrNone(this string text)
    { bool s = bool.TryParse(text, out var val); return s ? Some(val) : None<bool>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<bool> ParseBoolOrNone(this ReadOnlySpan<char> text)
    { bool s = bool.TryParse(text, out var val); return s ? Some(val) : None<bool>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateTime> ParseDateTimeOrNone(this string text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Some(val) : None<DateTime>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateTime> ParseDateTimeOrNone(this ReadOnlySpan<char> text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Some(val) : None<DateTime>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateOnly> ParseDateOnlyOrNone(this string text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Some(val) : None<DateOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateOnly> ParseDateOnlyOrNone(this ReadOnlySpan<char> text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Some(val) : None<DateOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<TimeOnly> ParseTimeOnlyOrNone(this string text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Some(val) : None<TimeOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<TimeOnly> ParseTimeOnlyOrNone(this ReadOnlySpan<char> text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Some(val) : None<TimeOnly>(); }


    // TryGetValue
    /// <summary>
    /// Returns Some of value from <paramref name="dictionary"/> with the given <paramref name="key"/> if exists; None if the key is absent.
    /// </summary>
    public static Opt<TValue> GetValueOrNone<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    { bool s = dictionary.TryGetValue(key, out var val); return s ? Some(val) : None<TValue>(); }
    /// <summary>
    /// Returns Some of value from <paramref name="dictionary"/> with the given <paramref name="key"/> if exists; None if the key is absent.
    /// </summary>
    public static Opt<TValue> GetValueOrNone<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
    { bool s = dictionary.TryGetValue(key, out var val); return s ? Some(val) : None<TValue>(); }


    // Collections
    /// <summary>
    /// Opt counterpart of FirstOrDefault, which returns the first non-null element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> enumerable)
    {
        if (typeof(T).IsClass)
        {
            foreach (var item in enumerable)
                if (item != null)
                    return new(item);
            return new();
        }
        else
        {
            foreach (var item in enumerable)
                return new(item);
            return new();
        }
    }
    /// <summary>
    /// Opt counterpart of FirstOrDefault, which returns the first Some element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<Opt<T>> enumerable)
    {
        foreach (var item in enumerable)
            if (item.IsSome)
                return item;
        return new();
    }
    /// <summary>
    /// Opt counterpart of LastOrDefault, which returns the last non-null element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<T> enumerable)
        => FirstOrNone(enumerable.Reverse());
    /// <summary>
    /// Opt counterpart of LastOrDefault, which returns the last Some element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<Opt<T>> enumerable)
        => FirstOrNone(enumerable.Reverse());
    /// <summary>
    /// Opt counterpart of FirstOrDefault over results collection, which returns the first Ok element of <paramref name="enumerable"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<Res<T>> enumerable)
    {
        foreach (var item in enumerable)
            if (item.IsOk)
                return new(item.value);
        return new();
    }
    /// <summary>
    /// Opt counterpart of LastOrDefault over results collection, which returns the last Ok element of <paramref name="enumerable"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<Res<T>> enumerable)
        => FirstOrNone(enumerable.Reverse());
    

    // Validation
    /// <summary>
    /// Returns Some(<paramref name="value"/>) if <paramref name="validator"/>(<paramref name="value"/>) returns true; None otherwise.
    /// </summary>
    public static Opt<T> Validate<T>(this T value, Func<T, bool> validator)
        => validator(value) ? Some(value) : None<T>();
}

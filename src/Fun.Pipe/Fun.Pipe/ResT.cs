namespace Fun;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Result type which can either be Ok with some value or Err.
/// </summary>
public readonly struct Res<T> : IEquatable<Res<T>>
{
    // Data
    internal readonly Res res;
    readonly Opt<T> value;
    // Prop
    /// <summary>
    /// <inheritdoc cref="Res.IsOk"/>
    /// </summary>
    public bool IsOk => res.IsOk;
    /// <summary>
    /// <inheritdoc cref="Res.IsErr"/>
    /// </summary>
    public bool IsErr => res.IsErr;


    // Ctor - Helper
    Res(Res res, Opt<T> value)
    {
        this.res = res;
        this.value = value;
    }
    internal static Res<T> ErrFrom(Res res) => new(res, Opt<T>.None);
    internal static Res<T> ErrFrom<TIn>(Res<TIn> res) => new(res.res, Opt<T>.None);
    // Ctor
    /// <summary>
    /// Returns Ok result with the given <paramref name="value"/>.
    /// </summary>
    public static Res<T> Ok(T value) => new(Res.Ok, Opt<T>.Some(value));
    /// <summary>
    /// <inheritdoc cref="Res.Err(string)"/>
    /// </summary>
    public static Res<T> Err(string message) => new(Res.Err(message), Opt<T>.None);
    /// <summary>
    /// <inheritdoc cref="Res.Err(Exception)"/>
    /// </summary>
    public static Res<T> Err(Exception exception) => new(Res.Err(exception), Opt<T>.None);
    /// <summary>
    /// <inheritdoc cref="Res.Err(string, Exception)"/>
    /// </summary>
    public static Res<T> Err(string message, Exception exception) => new(Res.Err(message, exception), Opt<T>.None);


    // Method
    /// <summary>
    /// <inheritdoc cref="Res.LogOnErr(bool)"/>
    /// </summary>
    public void LogOnErr(bool detailed = false) => res.LogOnErr(detailed);
    /// <summary>
    /// <inheritdoc cref="Res.ThrowOnErr"/>
    /// </summary>
    public void ThrowOnErr() => res.ThrowOnErr();
    /// <summary>
    /// Returns the result value when <see cref="IsOk"/>; or throws when <see cref="IsErr"/>.
    /// </summary>
    public T Unwrap()
    {
        if (IsErr)
            throw new ArgumentException("tried to unwrap Err");
        return value.Unwrap();
    }
    /// <summary>
    /// Returns the result value when <see cref="IsOk"/>; or returns the <paramref name="fallbackValue"/> when <see cref="IsErr"/>.
    /// </summary>
    /// <param name="fallbackValue"></param>
    public T Unwrap(T fallbackValue) => IsErr ? fallbackValue : value.Unwrap();


    // Common
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    public override string ToString() => IsOk ? $"Ok{value.Unwrap()}" : res.ToString();
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    /// <param name="detailed">Determines whether the error log wil be detailed or not.</param>
    public string ToString(bool detailed) => IsOk ? $"Ok{value.Unwrap()}" : res.ToString(detailed);
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="other"/>.
    /// </summary>
    public bool Equals(Res<T> other)
        => this == other;
    /// <summary>
    /// Returns whether <paramref name="first"/> is equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator ==(Res<T> first, Res<T> second)
    {
        if (first.IsErr)
            return second.IsErr;
        else
            return !second.IsErr && first.value == second.value;
    }
    /// <summary>
    /// Returns whether <paramref name="first"/> is not equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator !=(Res<T> first, Res<T> second)
    {
        if (first.IsErr)
            return !second.IsErr;
        else
            return second.IsErr || first.value != second.value;
    }
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="other"/>.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object obj)
        => (obj is Res<T>) ? (this == (Res<T>)obj) : false;
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
            => IsErr ? int.MinValue : value.GetHashCode();
}

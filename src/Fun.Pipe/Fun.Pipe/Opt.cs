namespace Fun;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Option type of <typeparamref name="T"/>: either None or Some value.
/// </summary>
public readonly struct Opt<T> : IEquatable<Opt<T>>
{
    // Data
    readonly T value;
    /// <summary>
    /// True if the option is None.
    /// </summary>
    public readonly bool IsNone;
    // Prop
    /// <summary>
    /// True if the option is Some value, which can be obtained by <see cref="Unwrap"/> or <see cref="Unwrap(T)"/>.
    /// </summary>
    public bool IsSome => !IsNone;


    // Ctor
    Opt(bool isNone, T value)
    {
        IsNone = isNone;
        this.value = value;
    }
    /// <summary>
    /// Returns None option.
    /// </summary>
    public static Opt<T> None => new(true, default);
    /// <summary>
    /// Returns Some option with the given <paramref name="value"/> of <typeparamref name="T"/>.
    /// </summary>
    public static Opt<T> Some(T value) => new(false, value);


    // Method
    /// <summary>
    /// Returns the value when <see cref="IsSome"/>; or throws when <see cref="IsNone"/>.
    /// </summary>
    public T Unwrap()
    {
        if (IsNone)
            throw new ArgumentException("tried to unwrap None");
        return value;
    }
    /// <summary>
    /// Returns the value when <see cref="IsSome"/>; or returns the <paramref name="fallbackValue"/> when <see cref="IsNone"/>.
    /// </summary>
    /// <param name="fallbackValue"></param>
    public T Unwrap(T fallbackValue)
        => IsNone ? fallbackValue : value;


    // Common
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    public override string ToString()
        => IsNone ? "None" : $"Some({value})";
    /// <summary>
    /// Returns whether this option is equal to the <paramref name="other"/>.
    /// </summary>
    public bool Equals(Opt<T> other)
        => this == other;
    /// <summary>
    /// Returns whether <paramref name="first"/> is equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator ==(Opt<T> first, Opt<T> second)
    {
        if (first.IsNone)
            return second.IsNone;
        else
            return !second.IsNone && first.value.Equals(second.value);
    }
    /// <summary>
    /// Returns whether <paramref name="first"/> is not equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator !=(Opt<T> first, Opt<T> second)
    {
        if (first.IsNone)
            return !second.IsNone;
        else
            return second.IsNone || !first.value.Equals(second.value);
    }
    /// <summary>
    /// Returns whether this option is equal to the <paramref name="other"/>.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object obj)
        => (obj is Opt<T>) ? (this == (Opt<T>)obj) : false;
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
        => IsNone ? int.MinValue : value.GetHashCode();
}

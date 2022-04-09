using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
namespace Fun;

/// <summary>
/// Immutable option type which can either be Some or None.
/// When the state <see cref="IsSome"/>, the option holds the valid value which can be extracted by <see cref="Unwrap()"/> (or <see cref="Unwrap(T)"/>) methods.
/// </summary>
public readonly struct Opt<T> : IEquatable<Opt<T>>
{
    // Data
    internal readonly T value;
    /// <summary>
    /// True if the option is None.
    /// </summary>
    public bool IsNone
        => !IsSome;
    // Propcd 
    /// <summary>
    /// True if the option is Some value, which can be obtained by <see cref="Unwrap()"/> or <see cref="Unwrap(T)"/>.
    /// </summary>
    public readonly bool IsSome;


    // Ctor
    internal Opt(T value)
    {
        if (typeof(T).IsClass)
        {
            if (value == null)
            {
                IsSome = false;
                this.value = default;
            }
            else
            {
                IsSome = true;
                this.value = value;
            }
        }
        else
        {
            IsSome = true;
            this.value = value;
        }
    }
    /// <summary>
    /// Option type of <typeparamref name="T"/>: either None or Some value.
    /// Parameterless ctor returns None; use 'Fun.Extensions.Some' or `Fun.Extensions.None` to construct options.
    /// Better to add `using static Fun.Extensions` and use `Some` and `None` directly.
    /// </summary>
    public Opt()
    {
        IsSome = false;
        value = default;
    }
    /// <summary>
    /// Implicitly converts to <paramref name="value"/> into <see cref="Opt{T}"/>.Some(<paramref name="value"/>).
    /// </summary>
    public static implicit operator Opt<T>(T value)
        => new(value);


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
    /// Returns the text representation of the option.
    /// /// </summary>
    public override string ToString()
        => IsNone ? "None" : $"Some({value})";
    /// <summary>
    /// Returns the text representation of the option; value will be <paramref name="format"/>ted when <see cref="IsSome"/>.
    /// </summary>
    public string ToString(string format)
    {
        if (IsNone)
            return "None";
        var method = typeof(T).GetMethod(nameof(ToString), new[] { typeof(string) });
        if (method == null)
            return $"Some({value})";
        string strValue = (string)method.Invoke(value, new[] { format });
        return $"Some({strValue})";
    }
    /// <summary>
    /// Returns whether this option is equal to the <paramref name="other"/>.
    /// </summary>
    public bool Equals(Opt<T> other)
        => this == other;
    /// <summary>
    /// Returns whether <paramref name="first"/> is equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator ==(Opt<T> first, Opt<T> second)
        => first.IsNone ? second.IsNone : second.IsSome && first.value.Equals(second.value);
    /// <summary>
    /// Returns whether <paramref name="first"/> is not equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator !=(Opt<T> first, Opt<T> second)
        => first.IsNone ? second.IsSome : second.IsNone || !first.value.Equals(second.value);
    /// <summary>
    /// Returns whether this option is equal to the <paramref name="obj"/>.
    /// </summary>
    public override bool Equals(object obj)
        => (obj is Opt<T>) && (this == (Opt<T>)obj);
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
        => IsNone ? int.MinValue : value.GetHashCode();
    /// <summary>
    /// Returns true if this <see cref="IsSome"/> and its unwrapped value is equal to the <paramref name="other"/>; false otherwise.
    /// </summary>
    public bool Equals(T other)
        => this == other;
}

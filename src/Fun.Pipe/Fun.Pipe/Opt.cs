namespace Fun;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Option type of <typeparamref name="T"/>: either None or Some value.
/// </summary>
public readonly struct Opt<T> : IEquatable<Opt<T>>, IEquatable<T>, IEquatable<Res<T>>
{
    // Data
    internal readonly T value;
    /// <summary>
    /// True if the option is None.
    /// </summary>
    public readonly bool IsNone;
    // Prop
    /// <summary>
    /// True if the option is Some value, which can be obtained by <see cref="Unwrap()"/> or <see cref="Unwrap(T)"/>.
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
    internal static Opt<T> None => new(true, default);
    /// <summary>
    /// Returns Some option with the given <paramref name="value"/> of <typeparamref name="T"/>.
    /// </summary>
    internal static Opt<T> Some(T value) => new(false, value);
    /// <summary>
    /// Return Some(value) if a non-null value could be parsed from <paramref name="textRepresentation"/> by the given <paramref name="parser"/>.
    /// Returns None otherwise.
    /// </summary>
    public static Opt<T> Parse(string textRepresentation, Func<string, T> parser)
    {
        var value = parser(textRepresentation);
        return value == null ? None : Some(value);
    }
    /// <summary>
    /// Tries to return Some(value) if a non-null value could be parsed without an exception from <paramref name="textRepresentation"/> by the given <paramref name="parser"/>.
    /// Returns None otherwise.
    /// </summary>
    public static Opt<T> TryParse(string textRepresentation, Func<string, T> parser)
    {
        try
        {
            var value = parser(textRepresentation);
            return value == null ? None : Some(value);
        }
        catch
        {
            return None;
        }
    }
    /// <summary>
    /// Implicitly returns Some option with the given <paramref name="value"/> of <typeparamref name="T"/>.
    /// </summary>
    public static implicit operator Opt<T>(T value) => Extensions.Some(value);


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
    /// <summary>
    /// Maps None to None; and Some(value) to Some(<paramref name="mapper"/>(value)); where value=this.Unwrap() is of type <typeparamref name="T"/>, and <paramref name="mapper"/>(value) is of type <typeparamref name="TOut"/>.
    /// </summary>
    public Opt<TOut> Map<TOut>(Func<T, TOut> mapper)
        => IsNone ? Opt<TOut>.None : Opt<TOut>.Some(mapper(value));
    /// <summary>
    /// Maps None to None; and Some(value) to <paramref name="mapper"/>(value); where value=this.Unwrap() is of type <typeparamref name="T"/>, and <paramref name="mapper"/>(value) might be None or Some of <typeparamref name="TOut"/>.
    /// </summary>
    public Opt<TOut> Map<TOut>(Func<T, Opt<TOut>> mapper)
        => IsNone ? Opt<TOut>.None : mapper(value);
    /// <summary>
    /// Does nothing when None; runs <paramref name="action"/>(value) when Some; where value=this.Unwrap() is of type <typeparamref name="T"/>.
    /// </summary>
    public void Run(Action action)
    {
        if (IsSome)
            action();
    }
    /// <summary>
    /// Does nothing when None; runs <paramref name="action"/>(value) when Some; where value=this.Unwrap() is of type <typeparamref name="T"/>.
    /// </summary>
    public void Run(Action<T> action)
    {
        if (IsSome)
            action(value);
    }
    /// <summary>
    /// Does nothing when Some; runs <paramref name="action"/> when None.
    /// </summary>
    public void RunWhenNone(Action action)
    {
        if (IsNone)
            action();
    }


    // Common
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// /// </summary>
    public override string ToString()
        => IsNone ? "None" : $"Some({value})";
    /// <summary>
    /// Returns the formatted string representation with the given <paramref name="format"/>.
    /// </summary>
    public string ToString(string format)
    {
        if (IsNone)
            return "None";

        var method = typeof(T).GetMethod(nameof(ToString), new[] { typeof(string) });
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
    /// Returns whether this option is equal to the <paramref name="obj"/>.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object obj)
        => (obj is Opt<T>) ? (this == (Opt<T>)obj) : false;
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
    /// <summary>
    /// Returns true if this <see cref="IsSome"/>, the other <see cref="Res{T}.IsOk"/>; and its unwrapped value is equal to <paramref name="other"/>'s unwrapped value; false otherwise.
    /// </summary>
    public bool Equals(Res<T> other)
        => IsSome && other.IsOk && value.Equals(other.Unwrap());
}

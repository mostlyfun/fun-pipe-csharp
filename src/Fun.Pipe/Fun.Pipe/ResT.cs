namespace Fun;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Result type which can either be Ok with some value or Err.
/// </summary>
public readonly struct Res<T> : IEquatable<Res<T>>, IEquatable<T>, IEquatable<Opt<T>>
{
    // Data
    internal readonly Res res;
    /// <summary>
    /// Value of the result which is None when <see cref="IsErr"/> and Some(T) when <see cref="IsOk"/>.
    /// </summary>
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
    /// <summary>
    /// <inheritdoc cref="Res.Exc"/>
    /// </summary>
    public Opt<Exception> Exc => res.Exc;
    /// <summary>
    /// <inheritdoc cref="Res.ErrMsg"/>
    /// </summary>
    public Opt<string> ErrMsg => res.ErrMsg;


    // Ctor - Helper
    Res(Res res, Opt<T> value)
    {
        this.res = res;
        this.value = value;
    }
    internal static Res<T> ErrFrom(Res res) => new(res, Extensions.None<T>());
    internal static Res<T> ErrFrom<TIn>(Res<TIn> res) => new(res.res, Extensions.None<T>());
    // Ctor
    /// <summary>
    /// Returns Ok result with the given <paramref name="value"/>.
    /// </summary>
    internal static Res<T> Ok(T value) => new(Res.Ok, Extensions.Some(value));
    /// <summary>
    /// <inheritdoc cref="Res.Err(string)"/>
    /// </summary>
    internal static Res<T> Err(string message = "") => new(Res.Err(message), Extensions.None<T>());
    /// <summary>
    /// <inheritdoc cref="Res.Err(Exception)"/>
    /// </summary>
    internal static Res<T> Err(Exception exception) => new(Res.Err(exception), Extensions.None<T>());
    /// <summary>
    /// <inheritdoc cref="Res.Err(string, Exception)"/>
    /// </summary>
    internal static Res<T> Err(string message, Exception exception) => new(Res.Err(message, exception), Extensions.None<T>());
    // Ctor - implicit
    /// <summary>
    /// Implicitly returns Ok result with the given <paramref name="value"/>.
    /// </summary>
    public static implicit operator Res<T>(T value) => Extensions.Ok(value);


    // Method
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
    /// <summary>
    /// Does nothing and returns self when <see cref="IsOk"/>; logs the error when <see cref="IsErr"/>.
    /// </summary>
    /// <param name="detailed">Determines whether the error log wil be detailed or not.</param>
    public Res<T> LogOnErr(bool detailed = false)
    {
        if (IsErr)
            res.LogOnErr(detailed);
        return this;
    }
    /// <summary>
    /// Does nothing and returns self when <see cref="IsOk"/>; throws when <see cref="IsErr"/>.
    /// </summary>
    public Res<T> ThrowOnErr()
    {
        if (IsErr)
            res.ThrowOnErr();
        return this;
    }
    /// <summary>
    /// Returns self when <see cref="IsOk"/>, Err with the <paramref name="newMessage"/> appended when <see cref="IsErr"/>.
    /// </summary>
    public Res<T> AddMessageWhenErr(string newMessage)
    {
        if (IsErr)
            res.AddMessageWhenErr(newMessage);
        return this;
    }
    /// <summary>
    /// Maps Err to Err; and Ok to Ok(<paramref name="mapper"/>) which is a result of <typeparamref name="TOut"/>.
    /// </summary>
    public Res<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        if (IsOk)
            return Res<TOut>.Ok(mapper(value.Unwrap()));
        else
            return Res<TOut>.ErrFrom(this);
    }
    /// <summary>
    /// Maps Err to Err; and Ok to <paramref name="getResult"/> which is a result of <typeparamref name="TOut"/>.
    /// </summary>
    public Res<TOut> Map<TOut>(Func<T, Res<TOut>> getResult)
        => IsOk ? getResult(value.Unwrap()) : Res<TOut>.ErrFrom(this);
    /// <summary>
    /// Does nothing when <see cref="IsErr"/>; runs <paramref name="action"/> when <see cref="IsOk"/>.
    /// Returns self.
    /// </summary>
    public Res<T> Run(Action action)
    {
        if (IsOk)
            action();
        return this;
    }
    /// <summary>
    /// Does nothing when <see cref="IsErr"/>; runs <paramref name="action"/> when <see cref="IsOk"/>.
    /// Returns self.
    /// </summary>
    public Res<T> Run(Action<T> action)
    {
        if (IsOk)
            action(value.Unwrap());
        return this;
    }
    /// <summary>
    /// Does nothing when <see cref="IsOk"/>; runs <paramref name="action"/> when <see cref="IsErr"/>.
    /// Returns self.
    /// </summary>
    public Res<T> RunWhenErr(Action action)
    {
        if (IsErr)
            action();
        return this;
    }


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
    /// Returns whether this result is equal to the <paramref name="obj"/>.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object obj)
        => (obj is Res<T>) ? (this == (Res<T>)obj) : false;
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
            => IsErr ? int.MinValue : value.GetHashCode();
    /// <summary>
    /// Returns true if this <see cref="IsOk"/> and its unwrapped value is equal to the <paramref name="other"/>; false otherwise.
    /// </summary>
    public bool Equals(T other)
        => this == other;
    /// <summary>
    /// Returns true if this <see cref="IsOk"/>, the other <see cref="Opt{T}.IsSome"/>; and its unwrapped value is equal to <paramref name="other"/>'s unwrapped value; false otherwise.
    /// </summary>
    public bool Equals(Opt<T> other)
        => IsOk && other.IsSome && value.Equals(other.Unwrap());
}

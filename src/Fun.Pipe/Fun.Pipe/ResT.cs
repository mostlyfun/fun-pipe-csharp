using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Fun;

/// <summary>
/// Immutable result type which can either be Ok or Err.
/// When the state <see cref="IsOk"/>, the result holds the valid value which can be extracted by <see cref="Unwrap()"/> (or <see cref="Unwrap(T)"/>) methods.
/// When the state <see cref="IsErr"/>, the result further holds Some <see cref="ErrorMessage"/>.
/// </summary>
public readonly struct Res<T>
{
    // Data
    internal readonly T value;
    readonly string errorMessage;
    // Prop
    /// <summary>
    /// True if the result is Ok; false otherwise.
    /// </summary>
    public bool IsOk => errorMessage == null;
    /// <summary>
    /// True if the result is Err; false otherwise.
    /// </summary>
    public bool IsErr => errorMessage != null;
    /// <summary>
    /// Returns the underlying error message if <see cref="IsErr"/>; None if <see cref="IsOk"/>;
    /// </summary>
    public Opt<string> ErrorMessage => errorMessage == null ? new() : new(errorMessage);


    // Ctor
    /// <summary>
    /// Parameterless ctor returns Err("not-initialized"), and hence, is not useful!
    /// Use 'Fun.Extensions.Ok' or `Fun.Extensions.Err` to construct options.
    /// Better to add `using static Fun.Extensions` and use `Ok` and `Err` directly.
    /// </summary>
    public Res()
    {
        errorMessage = "not-initialized";
        value = default;
    }
    internal Res(T value)
    {
        if (typeof(T).IsClass)
        {
            if (value == null)
            {
                errorMessage = $"null-is-passed-in-Res<{typeof(T).Name}>";
                this.value = default;
            }
            else
            {
                this.value = value;
                errorMessage = null;
            }
        }
        else
        {
            this.value = value;
            errorMessage = null;
        }
    }
    internal Res(string errorMessage, string when)
    {
        this.value = default;
        this.errorMessage = Res.GetErrorMessage(errorMessage, when);
    }
    internal Res(Exception exception, string when)
    {
        this.value = default;
        errorMessage = Res.GetExceptionMessage(exception, when);
    }
    /// <summary>
    /// Implicitly converts to <paramref name="value"/> into <see cref="Res{T}"/>.Ok(<paramref name="value"/>).
    /// </summary>
    public static implicit operator Res<T>(T value) => new(value);


    // Method
    /// <summary>
    /// Returns the value when <see cref="IsOk"/>; or throws when <see cref="IsErr"/>.
    /// </summary>
    public T Unwrap()
    {
        if (errorMessage != null)
            throw new ArgumentException("tried to unwrap None");
        return value;
    }
    /// <summary>
    /// Returns the value when <see cref="IsOk"/>; or returns the <paramref name="fallbackValue"/> when <see cref="IsErr"/>.
    /// </summary>
    public T Unwrap(T fallbackValue)
        => errorMessage == null ? value : fallbackValue;
    /// <summary>
    /// <inheritdoc cref="Res.MsgIfErr(string)"/>
    /// </summary>
    public Res<T> MsgIfErr(string errorMessage)
    {
        if (this.errorMessage == null)
            return this;
        string msg = this.errorMessage + Environment.NewLine + Res.GetErrorMessage(errorMessage, null);
        return new(msg, null);
    }
    /// <summary>
    /// <inheritdoc cref="Res.MsgIfErr(string, string)"/>
    /// </summary>
    public Res<T> MsgIfErr(string errorMessage, string when)
    {
        if (this.errorMessage == null)
            return this;
        string msg = this.errorMessage + Environment.NewLine + Res.GetErrorMessage(errorMessage, when);
        return new(msg, null);
    }


    // Common
    /// <summary>
    /// Returns the text representation of the option.
    /// </summary>
    public override string ToString() => IsOk ? $"Ok{value}" : $"Err({errorMessage})";
    /// <summary>
    /// Returns the text representation of the result; value will be <paramref name="format"/>ted when <see cref="IsOk"/>.
    /// </summary>
    /// <param name="format">Determines whether the error log wil be detailed or not.</param>
    public string ToString(string format)
    {
        if (IsErr)
            return $"Err({errorMessage})";
        var method = typeof(T).GetMethod(nameof(ToString), new[] { typeof(string) });
        if (method == null)
            return $"Ok({value})";
        string strValue = (string)method.Invoke(value, new[] { format });
        return $"Ok({strValue})";
    }
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="other"/>.
    /// </summary>
    public bool Equals(Res<T> other) => this == other;
    /// <summary>
    /// Returns whether <paramref name="first"/> is equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator ==(Res<T> first, Res<T> second) => first.IsErr ? false : !second.IsErr && first.value.Equals(second.value);
    /// <summary>
    /// Returns whether <paramref name="first"/> is not equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator !=(Res<T> first, Res<T> second) => first.IsErr ? true : second.IsErr || !first.value.Equals(second.value);
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="obj"/>.
    /// </summary>
    public override bool Equals(object obj) => (obj is Res<T>) ? (this == (Res<T>)obj) : false;
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode() => IsErr ? errorMessage.GetHashCode() : value.GetHashCode();
    /// <summary>
    /// Returns true if this <see cref="IsOk"/> and its unwrapped value is equal to the <paramref name="other"/>; false otherwise.
    /// </summary>
    public bool Equals(T other) => this == other;
    /// <summary>
    /// Returns true if this <see cref="IsOk"/>, the other <see cref="Opt{T}.IsSome"/>; and its unwrapped value is equal to <paramref name="other"/>'s unwrapped value; false otherwise.
    /// </summary>
    public bool Equals(Opt<T> other) => IsOk && other.IsSome && value.Equals(other.value);
}

namespace Fun;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

/// <summary>
/// Result type which can either be Ok or Err.
/// </summary>
public readonly struct Res : IEquatable<Res>
{
    // Data
    /// <summary>
    /// True if the result is Err; false otherwise.
    /// </summary>
    public readonly bool IsErr;
    /// <summary>
    /// Returns option of a possibly caught exception causing the result to be <see cref="IsErr"/>.
    /// </summary>
    public readonly Opt<Exception> Exc;
    /// <summary>
    /// Returns option of the message of a possible error causing the result to be <see cref="IsErr"/>.
    /// </summary>
    public readonly Opt<string> ErrMsg;
    // Prop
    /// <summary>
    /// True if the result is Ok; false otherwise.
    /// </summary>
    public bool IsOk => !IsErr;


    // Ctor - Helper
    Res(bool isErr, Opt<Exception> exc, Opt<string> msg)
    {
        IsErr = isErr;
        Exc = exc;
        this.ErrMsg = msg;
    }
    // Ctor
    /// <summary>
    /// Returns Ok result.
    /// </summary>
    public static Res Ok => new(false, Extensions.None<Exception>(), Extensions.None<string>());
    /// <summary>
    /// Returns Err result due to the given error <paramref name="message"/>.
    /// </summary>
    public static Res Err(string message = "") => new(true, Extensions.None<Exception>(), Extensions.Some(message));
    /// <summary>
    /// Returns Err result due to the given exception <paramref name="exception"/>.
    /// </summary>
    public static Res Err(Exception exception) => new(true, Extensions.Some(exception), Extensions.Some(exception.Message));
    /// <summary>
    /// Returns Err result due to the given error <paramref name="message"/> and exception <paramref name="exception"/>.
    /// </summary>
    public static Res Err(string message, Exception exception) => new(true, Extensions.Some(exception), Extensions.Some(message));

    // Method
    /// <summary>
    /// Does nothing and returns self when <see cref="IsOk"/>; logs the error when <see cref="IsErr"/>.
    /// </summary>
    /// <param name="detailed">Determines whether the error log wil be detailed or not.</param>
    public Res LogOnErr(bool detailed = false)
    {
        if (IsErr)
            Console.WriteLine($"[Err] {ToString(detailed)}");
        return this;
    }
    /// <summary>
    /// Does nothing and returns self when <see cref="IsOk"/>; throws when <see cref="IsErr"/>.
    /// </summary>
    public Res ThrowOnErr()
    {
        if (IsErr)
        {
            LogOnErr(true);
            throw new ArgumentException(ToString(), Exc.Unwrap(null));
        }
        return this;
    }
    /// <summary>
    /// Returns self when <see cref="IsOk"/>, Err with the <paramref name="newMessage"/> appended when <see cref="IsErr"/>.
    /// </summary>
    public Res AddMessageWhenErr(string newMessage)
    {
        if (IsOk)
            return this;
        if (ErrMsg.IsNone)
            return Err(newMessage);
        else
            return Err(ErrMsg.Unwrap() + "\n" + newMessage);
    }
    /// <summary>
    /// Maps Err to Err; and Ok to Ok(<paramref name="mapper"/>) which is a result of <typeparamref name="TOut"/>.
    /// </summary>
    public Res<TOut> Map<TOut>(Func<TOut> mapper)
    {
        if (IsOk)
            return Extensions.Ok(mapper());
        else
            return Res<TOut>.ErrFrom(this);
    }
    /// <summary>
    /// Maps Err to Err; and Ok to <paramref name="getResult"/> which is a result of <typeparamref name="TOut"/>.
    /// </summary>
    public Res<TOut> Map<TOut>(Func<Res<TOut>> getResult)
        => IsOk ? getResult() : Res<TOut>.ErrFrom(this);
    /// <summary>
    /// Does nothing when <see cref="IsErr"/>; runs <paramref name="action"/> when <see cref="IsOk"/>.
    /// Returns self.
    /// </summary>
    public Res Run(Action action)
    {
        if (IsOk)
            action();
        return this;
    }
    /// <summary>
    /// Does nothing when <see cref="IsOk"/>; runs <paramref name="action"/> when <see cref="IsErr"/>.
    /// Returns self.
    /// </summary>
    public Res RunWhenErr(Action action)
    {
        if (IsErr)
            action();
        return this;
    }


    // Common
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    public override string ToString()
        => IsOk ? "Ok" : $"Err({ErrMsg.Unwrap()})";
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    /// <param name="detailed">Determines whether the error log wil be detailed or not.</param>
    public string ToString(bool detailed)
    {
        if (!detailed || IsOk || Exc.IsNone)
            return ToString();

        var sb = new StringBuilder();
        sb.Append("Err(").Append(ErrMsg.Unwrap()).Append(')');
        var inner = Exc.Unwrap().InnerException;
        while (inner != null && inner.Message != null)
            sb.Append(">_ \n").Append(inner.Message);
        return sb.ToString();
    }
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="other"/>.
    /// </summary>
    public bool Equals(Res other)
        => this == other;
    /// <summary>
    /// Returns whether <paramref name="first"/> is equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator ==(Res first, Res second)
        => first.IsErr == second.IsErr;
    /// <summary>
    /// Returns whether <paramref name="first"/> is not equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator !=(Res first, Res second)
        => first.IsErr != second.IsErr;
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="obj"/>.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object obj)
        => (obj is Res) ? (this == (Res)obj) : false;
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
            => IsErr ? int.MinValue : int.MaxValue;
}

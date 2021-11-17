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
    readonly Opt<string> msg;
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
        this.msg = msg;
    }
    // Ctor
    /// <summary>
    /// Returns Ok result.
    /// </summary>
    public static Res Ok => new(false, Opt<Exception>.None, Opt<string>.None);
    /// <summary>
    /// Returns Err result due to the given error <paramref name="message"/>.
    /// </summary>
    public static Res Err(string message) => new(true, Opt<Exception>.None, Opt<string>.Some(message));
    /// <summary>
    /// Returns Err result due to the given exception <paramref name="exception"/>.
    /// </summary>
    public static Res Err(Exception exception) => new(true, Opt<Exception>.Some(exception), Opt<string>.Some(exception.Message));
    /// <summary>
    /// Returns Err result due to the given error <paramref name="message"/> and exception <paramref name="exception"/>.
    /// </summary>
    public static Res Err(string message, Exception exception) => new(true, Opt<Exception>.Some(exception), Opt<string>.Some(message));


    // Method
    /// <summary>
    /// Does nothing when <see cref="IsOk"/>; logs the error when <see cref="IsErr"/>.
    /// </summary>
    /// <param name="detailed">Determines whether the error log wil be detailed or not.</param>
    public void LogOnErr(bool detailed = false)
    {
        if (IsOk)
            return;
        Console.WriteLine($"[Err] {ToString(detailed)}");
    }
    /// <summary>
    /// Does nothing when <see cref="IsOk"/>; throws when <see cref="IsErr"/>.
    /// </summary>
    public void ThrowOnErr()
    {
        if (IsOk)
            return;
        LogOnErr(true);
        throw new ArgumentException(ToString(), Exc.Unwrap(null));
    }


    // Common
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    public override string ToString()
        => IsOk ? "Ok" : $"Err({msg.Unwrap()})";
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    /// <param name="detailed">Determines whether the error log wil be detailed or not.</param>
    public string ToString(bool detailed)
    {
        if (!detailed || IsOk || Exc.IsNone)
            return ToString();

        var sb = new StringBuilder();
        sb.Append("Err(").Append(msg.Unwrap()).Append(')');
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
    /// Returns whether this result is equal to the <paramref name="other"/>.
    /// </summary>
    public override bool Equals([NotNullWhen(true)] object obj)
        => (obj is Res) ? (this == (Res)obj) : false;
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
            => IsErr ? int.MinValue : int.MaxValue;
}

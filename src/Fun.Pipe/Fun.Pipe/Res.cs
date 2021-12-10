using System;
using System.Text;
namespace Fun;

/// <summary>
/// Immutable result type which can either be Ok or Err.
/// When the state <see cref="IsErr"/>, the result further holds Some <see cref="ErrorMessage"/>.
/// </summary>
public readonly struct Res
{
    // Data
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
    /// Returns the underlying error message which is Some when the result <see cref="IsErr"/>; None when <see cref="IsOk"/>;
    /// </summary>
    public Opt<string> ErrorMessage => errorMessage == null ? new() : new(errorMessage);

    // Ctor
    /// <summary>
    /// Result type which can either be Ok or Err.
    /// Parameterless ctor returns Ok; use 'Fun.Extensions.Ok' or `Fun.Extensions.Err` to construct options.
    /// Better to add `using static Fun.Extensions` and use `Ok` and `Err` directly.
    /// </summary>
    public Res() => errorMessage = null;
    internal Res(string errorMessage, string when = null) => this.errorMessage = GetErrorMessage(errorMessage, when);
    internal Res(Exception exception, string when = null) => this.errorMessage = GetExceptionMessage(exception, when);


    // Method
    /// <summary>
    /// Appends the provided <paramref name="errorMessage"/> if the result <see cref="IsErr"/>.
    /// Does nothing when <see cref="IsOk"/>.
    /// Returns self.
    /// </summary>
    public Res MsgIfErr(string errorMessage)
    {
        if (this.errorMessage == null)
            return this;
        string msg = this.errorMessage + Environment.NewLine + Res.GetErrorMessage(errorMessage, null);
        return new(msg, null);
    }
    /// <summary>
    /// Appends the provided <paramref name="errorMessage"/> related to the operation at <paramref name="when"/> if the result <see cref="IsErr"/>.
    /// Does nothing when <see cref="IsOk"/>.
    /// Returns self.
    /// </summary>
    public Res MsgIfErr(string errorMessage, string when)
    {
        if (this.errorMessage == null)
            return this;
        string msg = this.errorMessage + Environment.NewLine + Res.GetErrorMessage(errorMessage, when);
        return new(msg, null);
    }


    // Common
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    public override string ToString() => IsOk ? "Ok" : $"Err({errorMessage})";
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="other"/>.
    /// </summary>
    public bool Equals(Res other) => this == other;
    /// <summary>
    /// Returns whether <paramref name="first"/> is equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator ==(Res first, Res second) => first.IsOk && second.IsOk;
    /// <summary>
    /// Returns whether <paramref name="first"/> is not equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator !=(Res first, Res second) => first.IsErr || second.IsErr;
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="obj"/>.
    /// </summary>
    public override bool Equals(object obj) => (obj is Res) ? (this == (Res)obj) : false;
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode() => IsErr ? errorMessage.GetHashCode() : int.MaxValue;


    // Method Helper
    internal static string GetErrorMessage(string errorMessage, string when)
    {
        if (errorMessage == null)
            errorMessage = string.Empty;
        return when == null ? errorMessage : $"[{when}] {errorMessage}";
    }
    internal static string GetExceptionMessage(Exception exception, string when)
    {
        var sb = new StringBuilder();
        if (when != null)
            sb.Append("[exc@").Append(when).Append("] ");
        else
            sb.Append("[exc] ");
        AppendException(sb, exception);
        var inner = exception.InnerException;
        while (inner != null)
        {
            sb.Append('\n');
            AppendException(sb, inner);
            inner = inner.InnerException;
        }
        return sb.ToString();
    }
    static void AppendException(StringBuilder sb, Exception exception)
        => sb.Append(exception.GetType().Name).Append(": ").Append(exception.Message);
}

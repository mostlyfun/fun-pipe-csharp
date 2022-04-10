using System.Text;
using System.Text.RegularExpressions;
namespace Fun;

/// <summary>
/// Immutable result type which can either be Ok or Err.
/// When the state <see cref="IsErr"/>, the result further holds Some <see cref="ErrorMessage"/>.
/// </summary>
public readonly struct Res : IEquatable<Res>
{
    // Data
    readonly string errorMessage;
    // Prop
    /// <summary>
    /// True if the result is Ok; false otherwise.
    /// </summary>
    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => errorMessage == null;
    }
    /// <summary>
    /// True if the result is Err; false otherwise.
    /// </summary>
    public bool IsErr
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => errorMessage != null;
    }
    /// <summary>
    /// Returns the underlying error message which is Some when the result <see cref="IsErr"/>; None when <see cref="IsOk"/>;
    /// </summary>
    public Opt<string> ErrorMessage
        => errorMessage == null ? new() : new(errorMessage);


    // Ctor
    /// <summary>
    /// Result type which can either be Ok or Err.
    /// Parameterless ctor returns Ok; use 'Fun.Extensions.Ok' or `Fun.Extensions.Err` to construct options.
    /// Better to add `using static Fun.Extensions` and use `Ok` and `Err` directly.
    /// </summary>
    public Res()
    {
        errorMessage = "default-ctor-error";
    }
    internal Res(string errorMessage) => this.errorMessage = errorMessage;
    internal Res(string errorMessage, string when) => this.errorMessage = GetErrorMessage(errorMessage, when);
    internal Res(Exception exception, string when) => errorMessage = GetExceptionMessage(exception, when);


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
        string msg = string.Format("{0}\n: {1}", this.errorMessage, GetErrorMessage(errorMessage, null));
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
        string msg = string.Format("{0}\n: {1}", this.errorMessage, GetErrorMessage(errorMessage, when));
        return new(msg, null);
    }


    // Common
    /// <summary>
    /// Converts the option to its equivalent string representation.
    /// </summary>
    public override string ToString()
        => IsOk ? "Ok" : $"Err({errorMessage})";
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="other"/>.
    /// </summary>
    public bool Equals(Res other)
        => this == other;
    /// <summary>
    /// Returns whether <paramref name="first"/> is equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator ==(Res first, Res second)
        => first.IsOk && second.IsOk;
    /// <summary>
    /// Returns whether <paramref name="first"/> is not equal to <paramref name="second"/>.
    /// </summary>
    public static bool operator !=(Res first, Res second)
        => first.IsErr || second.IsErr;
    /// <summary>
    /// Returns whether this result is equal to the <paramref name="obj"/>.
    /// </summary>
    public override bool Equals(object obj)
        => (obj is Res) && (this == (Res)obj);
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
        => IsErr ? errorMessage.GetHashCode() : int.MaxValue;


    // Method Helper
    const string patternStackTrace = @"( at )|( in )|(:line )";
    internal static string GetErrorMessage(string errorMessage, string when)
    {
        if (errorMessage == null)
            errorMessage = string.Empty;
        return when == null ? errorMessage : string.Format("[{0}] {1}", when, errorMessage);
    }
    internal static string GetExceptionMessage(Exception exception, string when)
    {
        var sb = new StringBuilder();

        sb.Append("Exception. ");
        if (when != null) sb.Append(when);
        sb.AppendLine();

        sb.Append("  ! ").Append(exception.GetType().Name).Append(": ").AppendLine(exception.Message);
        var exc = exception.InnerException;
        while (exc != null)
        {
            sb.Append("  ! ").Append(exc.GetType().Name).Append(": ").AppendLine(exc.Message);
            exc = exc.InnerException;
        }

        if (exception.StackTrace != null)
        {
            var stack = exception.StackTrace.Split(Environment.NewLine);
            foreach (var line in stack)
            {
                var parts = Regex.Split(line, patternStackTrace);
                if (parts.Length < 7) { sb.Append("    -> ").AppendLine(line.Trim()); continue; }
                int indLastSlash = parts[4].LastIndexOf('\\');
                //string file = indLastSlash < 1 ? parts[4] : parts[4].Substring(indLastSlash + 1);
                sb.Append("    -> ").Append(parts[2]).Append(" | ").AppendLine(parts[6]);
            }
        }
        return sb.ToString();
    }
}

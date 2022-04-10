namespace Fun;

/// <summary>
/// Immutable option type which can either be Some or None.
/// When the state <see cref="IsSome"/>, the option holds the valid value which can be extracted by <see cref="Unwrap()"/> (or <see cref="Unwrap(T)"/>) methods.
/// </summary>
public readonly struct Opt<T> : IEquatable<T>, IEquatable<Opt<T>>, IEquatable<Res<T>>
{
    // Data
    internal readonly T value;
    /// <summary>
    /// True if the option is None.
    /// </summary>
    public bool IsNone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !IsSome;
    }
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap()
    {
        if (IsNone)
            throw new ArgumentException("tried to unwrap None");
        return value!;
    }
    /// <summary>
    /// Returns the value when <see cref="IsSome"/>; or returns the <paramref name="fallbackValue"/> when <see cref="IsNone"/>.
    /// </summary>
    /// <param name="fallbackValue"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap(T fallbackValue)
        => IsNone ? fallbackValue : value!;
    /// <summary>
    /// Returns the value when <see cref="IsSome"/>; or returns <paramref name="lazyFallbackValue"/>() when <see cref="IsNone"/>.
    /// </summary>
    /// <param name="lazyFallbackValue"></param>
    public T Unwrap(Func<T> lazyFallbackValue)
        => IsNone ? lazyFallbackValue() : value!;
    /// <summary>
    /// Returns the value when <see cref="IsSome"/>; or returns <paramref name="lazyFallbackValue"/>() when <see cref="IsNone"/>.
    /// </summary>
    /// <param name="lazyFallbackValue"></param>
    public Task<T> Unwrap(Func<Task<T>> lazyFallbackValue)
        => IsNone ? lazyFallbackValue() : Task.FromResult(value!);
    /// <summary>
    /// Returns the value when <see cref="IsSome"/>; throws with the given <paramref name="errorMessage"/> when <see cref="IsNone"/>.
    /// </summary>
    public T UnwrapOrThrow(string errorMessage)
    {
        if (IsNone)
            throw new ArgumentException(errorMessage);
        return value!;
    }


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
    /// Returns true if both values are <see cref="IsSome"/> and their unwrapped values are equal; false otherwise.
    /// </summary>
    public static bool operator ==(Opt<T> first, Opt<T> second)
        => first.IsSome && second.IsSome && first.value.Equals(second.value);
    /// <summary>
    /// Returns true if either value <see cref="IsNone"/> or their unwrapped values are not equal; false otherwise.
    /// </summary>
    public static bool operator !=(Opt<T> first, Opt<T> second)
        => first.IsNone || second.IsNone || !first.value.Equals(second.value);
    /// <summary>
    /// Returns true if lhs <see cref="IsSome"/> and its unwrapped value is equal to the rhs; false otherwise.
    /// </summary>
    public static bool operator ==(Opt<T> first, T second)
        => first.IsSome && first.value.Equals(second);
    /// <summary>
    /// Returns true if lhs <see cref="IsNone"/> or its unwrapped value is not equal to the rhs; false otherwise.
    /// </summary>
    public static bool operator !=(Opt<T> first, T second)
        => first.IsNone || !first.value.Equals(second);
    /// <summary>
    /// Returns true if rhs <see cref="IsSome"/> and its unwrapped value is equal to the lhs; false otherwise.
    /// </summary>
    public static bool operator ==(T first, Opt<T> second)
        => second.IsSome && second.value.Equals(first);
    /// <summary>
    /// Returns true if rhs <see cref="IsNone"/> or its unwrapped value is not equal to the lhs; false otherwise.
    /// </summary>
    public static bool operator !=(T first, Opt<T> second)
        => second.IsNone || !second.value.Equals(first);
    /// <summary>
    /// Returns true if lhs.IsSome and rhs.IsOk and their unwrapped values are equal; false otherwise.
    /// </summary>
    public static bool operator ==(Opt<T> first, Res<T> second)
        => first.IsSome && second.IsOk && first.value.Equals(second.value);
    /// <summary>
    /// Returns true if lhs.IsNone and rhs.IsErr and their unwrapped values are not equal; false otherwise.
    /// </summary>
    public static bool operator !=(Opt<T> first, Res<T> second)
        => first.IsNone || second.IsErr || !first.value.Equals(second.value);
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
    /// <inheritdoc cref="operator ==(Opt{T}, Opt{T})"/>
    /// </summary>
    public bool Equals(Opt<T> other)
        => this == other;
    /// <summary>
    /// <inheritdoc cref="operator ==(Opt{T}, Res{T})"/>
    /// </summary>
    public bool Equals(Res<T> other)
        => this == other;
    /// <summary>
    /// <inheritdoc cref="operator ==(Opt{T}, T)"/>
    /// </summary>
    public bool Equals(T other)
        => this == other;
}

namespace Fun;

/// <summary>
/// Defines the way errors will be handled (None/Log/Throw), the behavior will be carried on succeeding pipe states.
/// </summary>
public enum OnErr
{
    /// <summary>
    /// Errors are silently suppressed; the execution continues while succeeding pipe-runs are bypassed.
    /// </summary>
    None,
    /// <summary>
    /// Errors are logged; the execution continues while succeeding pipe-runs are bypassed.
    /// </summary>
    Log,
    /// <summary>
    /// An exception is thrown immediately on error.
    /// </summary>
    Throw,
}

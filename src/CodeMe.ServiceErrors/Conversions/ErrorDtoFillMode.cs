namespace CodeMe.ServiceErrors.Conversions;

/// <summary>
/// Fill mode for <see cref="ServiceErrorDto"/> fields.
/// </summary>
public enum ErrorDtoFillMode
{
    /// <summary>
    /// Always fill.
    /// </summary>
    Always,

    /// <summary>
    /// Fill for unknown error codes only.
    /// </summary>
    UnknownErrorsOnly,

    /// <summary>
    /// Do not fill.
    /// </summary>
    Newer
}
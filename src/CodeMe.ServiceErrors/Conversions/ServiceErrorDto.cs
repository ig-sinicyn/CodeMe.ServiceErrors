namespace CodeMe.ServiceErrors.Conversions;

/// <summary>
/// Serializable service error.
/// </summary>
public class ServiceErrorDto
{
    /// <summary>
    /// Application name. Binds to the <see cref="ErrorGroupUri.Application"/>.
    /// </summary>
    public string? Application { get; init; }

    /// <summary>
    /// Category. Binds to the <see cref="ErrorGroupUri.Category"/>.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Error code. Binds to the <see cref="ErrorUri.Code"/>.
    /// </summary>
    public string Code { get; init; } = "";

    /// <summary>
    /// Error status. Binds to the <see cref="ErrorDescriptor.StatusCode"/>.
    /// </summary>
    public ErrorStatusCode StatusCode { get; init; }

    /// <summary>
    /// Error detail message. Binds to the <see cref="ServiceError.Message"/>.
    /// </summary>
    public string Message { get; init; } = "";
}
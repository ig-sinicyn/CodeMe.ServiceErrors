namespace CodeMe.ServiceErrors.Conversions;

/// <summary>
/// Settings for conversions between the <see cref="ServiceErrorDto"/> and <see cref="ServiceError"/>.
/// </summary>
public interface IServiceErrorFactoryOptions
{
    /// <summary>
    /// Error group for unknown error codes.
    /// </summary>
    ErrorGroupUri DefaultGroup { get; }

    /// <summary>
    /// Fill mode for <see cref="ServiceErrorDto.Application"/>.
    /// </summary>
    ErrorDtoFillMode ApplicationFillMode { get; }

    /// <summary>
    /// Fill mode for <see cref="ServiceErrorDto.Category"/>.
    /// </summary>
    ErrorDtoFillMode CategoryFillMode { get; }

    /// <summary>
    /// Well-known error code mapping (<see cref="ErrorUri.Code"/>).
    /// </summary>
    IReadOnlyDictionary<string, ErrorDescriptor> ErrorCodeMapping { get; }

    /// <summary>
    /// Default exception factory.
    /// </summary>
    Func<ServiceError, IServiceException>? DefaultErrorFactory { get; }

    /// <summary>
    /// Exception factories for well-known error types.
    /// </summary>
    IReadOnlyDictionary<ErrorUri, Func<ServiceError, IServiceException>> ErrorFactories { get; }

    /// <summary>
    /// Exception factories for well-known error groups.
    /// </summary>
    IReadOnlyDictionary<ErrorGroupUri, Func<ServiceError, IServiceException>> ErrorGroupFactories { get; }
}
namespace CodeMe.ServiceErrors.Conversions;

/// <summary>
/// Immutable <see cref="IServiceErrorFactoryOptions"/> implementation.
/// Used mostly for testing and as a default options instance for <see cref="DefaultServiceErrorFactory"/>.
/// </summary>
public sealed record DefaultServiceErrorFactoryOptions(
    ErrorGroupUri DefaultGroup,
    ErrorDtoFillMode ApplicationFillMode,
    ErrorDtoFillMode CategoryFillMode,
    IReadOnlyDictionary<string, ErrorDescriptor> ErrorCodeMapping,
    Func<ServiceError, IServiceException>? DefaultErrorFactory,
    IReadOnlyDictionary<ErrorUri, Func<ServiceError, IServiceException>> ErrorFactories,
    IReadOnlyDictionary<ErrorGroupUri, Func<ServiceError, IServiceException>> ErrorGroupFactories)
    : IServiceErrorFactoryOptions;
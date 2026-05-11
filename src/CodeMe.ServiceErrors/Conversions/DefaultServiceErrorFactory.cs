namespace CodeMe.ServiceErrors.Conversions;

internal sealed class DefaultServiceErrorFactory : IServiceErrorFactory
{
    private readonly IServiceErrorFactoryOptions _options;

    public DefaultServiceErrorFactory(IServiceErrorFactoryOptions options)                                             
    {
        _options = options;
    }

    public ErrorGroupUri FallbackErrorGroup => _options.DefaultGroup;

    public ServiceErrorDto CreateDto(ServiceError error)
    {
        var errorType = error.Descriptor.Type;
        var registeredType = _options.ErrorCodeMapping.GetValueOrDefault(errorType.Code)?.Type;
        var isWellKnown = errorType == registeredType;

        var application = GetDtoField(
            _options.ApplicationFillMode,
            errorType.Group.Application,
            _options.DefaultGroup.Application,
            isWellKnown);
        var category = GetDtoField(
            _options.CategoryFillMode,
            errorType.Group.Category,
            _options.DefaultGroup.Category,
            isWellKnown);

        return new ServiceErrorDto
        {
            Application = application,
            Category = category,
            Code = errorType.Code,
            StatusCode = error.Descriptor.StatusCode,
            Message = error.Message
        };
    }

    private static string? GetDtoField(
        ErrorDtoFillMode fillMode,
        string? value,
        string? defaultValue,
        bool isWellKnown) =>
        fillMode switch
        {
            ErrorDtoFillMode.Always => value,
            ErrorDtoFillMode.UnknownErrorsOnly =>
                isWellKnown || string.Equals(value, defaultValue, ErrorGroupUri.UriPartComparison)
                    ? null
                    : value,
            ErrorDtoFillMode.Newer => null,
            _ => throw new ArgumentOutOfRangeException(nameof(fillMode), fillMode, null)
        };

    public ServiceError CreateError(ServiceErrorDto error) =>
        new(
            Descriptor: CreateErrorDescriptor(error),
            Message: error.Message);

    public ServiceError CreateError(Exception exception)
    {
        if (exception is IServiceException serviceException)
        {
            return serviceException.Error;
        }

        var errorType = ErrorUri.Create(
            _options.DefaultGroup,
            ErrorUriConvert.ExceptionTypeToErrorCode(exception.GetType()));

        var descriptor = new ErrorDescriptor(Type: errorType, StatusCode: ErrorStatusCode.Internal);

        return new ServiceError(
            Descriptor: descriptor,
            Message: exception.Message)
        {
            InnerException = exception
        };
    }

    private ErrorDescriptor CreateErrorDescriptor(ServiceErrorDto error)
    {
        var errorType = ErrorUri.Create(
            ErrorGroupUri.Create(
                error.Application ?? _options.DefaultGroup.Application,
                error.Category ?? _options.DefaultGroup.Category),
            error.Code);

        if (_options.ErrorCodeMapping.TryGetValue(errorType.Code, out var descriptor)
            && descriptor.Type == errorType)
        {
            return descriptor;
        }

        return new ErrorDescriptor(Type: errorType, StatusCode: error.StatusCode);
    }

    public IServiceException CreateException(ServiceErrorDto error) =>
        CreateException(CreateError(error));

    public IServiceException CreateException(ServiceError error)
    {
        // Get factory by type
        if (_options.ErrorFactories.TryGetValue(error.Descriptor.Type, out var factory))
        {
            return factory(error);
        }

        // Get factory by group
        var errorGroupFactories = _options.ErrorGroupFactories;
        if (errorGroupFactories.Count > 0)
        {
            var currentGroup = error.Descriptor.Type.Group;
            while (currentGroup != null)
            {
                if (errorGroupFactories.TryGetValue(currentGroup, out var groupFactory))
                {
                    return groupFactory(error);
                }

                currentGroup = currentGroup.ParentGroup;
            }
        }

        // Default factory
        return _options.DefaultErrorFactory?.Invoke(error)
          ?? new ServiceException(error);
    }
}
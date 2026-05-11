using CodeMe.ServiceErrors.Conversions;

namespace CodeMe.ServiceErrors.DependencyInjection;

public class ServiceErrorFactoryOptions
{
    public required ErrorGroupUri DefaultGroup { get; set; }

    public ErrorDtoFillMode ApplicationFillMode { get; set; }

    public ErrorDtoFillMode CategoryFillMode { get; set; }

    public Dictionary<string, ErrorDescriptor> ErrorCodeMapping =
        new(StringComparer.FromComparison(ErrorGroupUri.UriPartComparison));

    public Func<ServiceError, IServiceException>? DefaultErrorFactory { get; set; }

    public Dictionary<ErrorUri, Func<ServiceError, IServiceException>> ErrorFactories { get; } = new();

    public Dictionary<ErrorGroupUri, Func<ServiceError, IServiceException>> ErrorGroupFactories { get; } = new();
}
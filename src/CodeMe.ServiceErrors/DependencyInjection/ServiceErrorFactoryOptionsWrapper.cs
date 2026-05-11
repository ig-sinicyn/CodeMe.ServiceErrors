using CodeMe.ServiceErrors.Conversions;

namespace CodeMe.ServiceErrors.DependencyInjection;

public class ServiceErrorFactoryOptionsWrapper : IServiceErrorFactoryOptions
{
    public required ServiceErrorFactoryOptions Source { get; set; }

    public ErrorGroupUri DefaultGroup => Source.DefaultGroup;

    public ErrorDtoFillMode ApplicationFillMode => Source.ApplicationFillMode;

    public ErrorDtoFillMode CategoryFillMode => Source.CategoryFillMode;

    public IReadOnlyDictionary<string, ErrorDescriptor> ErrorCodeMapping => Source.ErrorCodeMapping;

    public Func<ServiceError, IServiceException>? DefaultErrorFactory => Source.DefaultErrorFactory;

    public IReadOnlyDictionary<ErrorUri, Func<ServiceError, IServiceException>> ErrorFactories => Source.ErrorFactories;

    public IReadOnlyDictionary<ErrorGroupUri, Func<ServiceError, IServiceException>> ErrorGroupFactories =>
        Source.ErrorGroupFactories;
}
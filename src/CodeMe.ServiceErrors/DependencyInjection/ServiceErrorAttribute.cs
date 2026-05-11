namespace CodeMe.ServiceErrors.DependencyInjection;

/// <summary>
/// Marks a field of type <see cref="ErrorDescriptor"/>, <see cref="ErrorUri"/>, <see cref="ErrorGroupUri"/>.
/// associated exception.
/// </summary>
#pragma warning disable CA1813
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class ServiceErrorAttribute : Attribute
#pragma warning restore CA1813
{
    /// <summary>
    /// Marks a field of type <see cref="ErrorDescriptor"/>, <see cref="ErrorUri"/>, <see cref="ErrorGroupUri"/>.
    /// associated exception.
    /// </summary>
    public ServiceErrorAttribute(Type exceptionType)
    {
        ExceptionType = exceptionType;
    }

    /// <summary>
    /// The exception type that will be created for errors with the specified description / type / error group.
    /// </summary>
    public Type ExceptionType { get; }
}
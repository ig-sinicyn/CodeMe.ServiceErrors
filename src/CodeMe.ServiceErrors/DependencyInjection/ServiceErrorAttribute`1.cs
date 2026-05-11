namespace CodeMe.ServiceErrors.DependencyInjection;

/// <summary>
/// Marks a field of type <see cref="ErrorDescriptor"/>, <see cref="ErrorUri"/>, <see cref="ErrorGroupUri"/>.
/// associated exception.
/// </summary>
#pragma warning disable CA1813
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class ServiceErrorAttribute<TException> : ServiceErrorAttribute
    where TException : Exception, IServiceException
#pragma warning restore CA1813
{
    /// <summary>
    /// Marks a field of type <see cref="ErrorDescriptor"/>, <see cref="ErrorUri"/>, <see cref="ErrorGroupUri"/>.
    /// associated exception.
    /// </summary>
    public ServiceErrorAttribute() : base(typeof(TException))
    {
    }
}
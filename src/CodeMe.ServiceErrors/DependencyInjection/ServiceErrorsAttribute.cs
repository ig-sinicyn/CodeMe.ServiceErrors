using CodeMe.ServiceErrors.Conversions;

namespace CodeMe.ServiceErrors.DependencyInjection;

/// <summary>
/// Marks a static class containing error details for configuring <see cref="IServiceErrorFactory"/>.
/// associated exception.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ServiceErrorsAttribute : Attribute
{
    // See the attribute guidelines at
    //  http://go.microsoft.com/fwlink/?LinkId=85236
    public ServiceErrorsAttribute()
    {
        // TODO: Implement code here
        throw new NotImplementedException();
    }
}
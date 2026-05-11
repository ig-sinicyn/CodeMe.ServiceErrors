namespace CodeMe.ServiceErrors;

/// <summary>
/// Base exception for <see cref="ServiceError"/>.
/// </summary>
public class ServiceException : Exception, IServiceException
{
    /// <summary>
    /// Base exception for <see cref="ServiceError"/>.
    /// </summary>
    public ServiceException(ServiceError error) : base(error.Message, error.InnerException)
    {
        Error = error;
    }

    /// <inheritdoc/>.
    public ServiceError Error { get; }
}
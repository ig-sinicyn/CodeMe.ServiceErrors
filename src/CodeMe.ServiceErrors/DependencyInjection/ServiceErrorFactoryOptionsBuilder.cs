using System.Reflection;
using System.Runtime.CompilerServices;

namespace CodeMe.ServiceErrors.DependencyInjection;

public class ServiceErrorFactoryOptionsBuilder
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void FillOptions(ServiceErrorFactoryOptions options, string assemblyNamePrefix)
    {
        var visitedAssemblies = new HashSet<AssemblyName>();
        var rootAssembly = Assembly.GetEntryAssembly()!;
        FillOptionsTraverse(options, rootAssembly, assemblyNamePrefix, visitedAssemblies);
    }

    private static void FillOptionsTraverse(
        ServiceErrorFactoryOptions options,
        Assembly assembly,
        string assemblyNamePrefix,
        HashSet<AssemblyName> visitedAssemblies)
    {
        var assemblyName = assembly.GetName();
        if (!visitedAssemblies.Add(assemblyName))
        {
            return;
        }

        if (assemblyName.Name?.StartsWith(assemblyNamePrefix, StringComparison.OrdinalIgnoreCase) ?? false)
        {
            FillOptions(options, assembly);
        }

        foreach (var referencedName in assembly.GetReferencedAssemblies())
        {
            if (referencedName.Name?.StartsWith(assemblyNamePrefix, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var referencedAssembly = Assembly.Load(referencedName);
                FillOptionsTraverse(options, referencedAssembly, assemblyNamePrefix, visitedAssemblies);
            }
        }
    }

    public static void FillOptions(ServiceErrorFactoryOptions options, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(x => x.GetCustomAttribute<ServiceErrorsAttribute>() != null);
        foreach (var type in types)
        {
            FillOptions(options, type);
        }
    }

    public static void FillOptions(ServiceErrorFactoryOptions options, Type errorDescriptorsType)
    {
        var mappingFields = errorDescriptorsType.GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var mapping in mappingFields)
        {
            if (mapping.FieldType == typeof(ErrorDescriptor))
            {
                var descriptor = (ErrorDescriptor)mapping.GetValue(null)!;
                options.ErrorCodeMapping.Add(descriptor.Type.Code, descriptor);
                if (mapping.GetCustomAttribute<ServiceErrorAttribute>() is
                    { } descriptorAttribute)
                {
                    options.ErrorFactories.Add(descriptor.Type, GetExceptionFactory(descriptorAttribute.ExceptionType));
                }
            }

            if (mapping.FieldType == typeof(ErrorGroupUri) && mapping.GetCustomAttribute<ServiceErrorAttribute>() is
                    { } groupAttribute)
            {
                var group = (ErrorGroupUri)mapping.GetValue(null)!;
                options.ErrorGroupFactories.Add(group, GetExceptionFactory(groupAttribute.ExceptionType));
                continue;
            }

            if (mapping.FieldType == typeof(ErrorUri) && mapping.GetCustomAttribute<ServiceErrorAttribute>() is
                    { } typeAttribute)
            {
                var type = (ErrorUri)mapping.GetValue(null)!;
                options.ErrorFactories.Add(type, GetExceptionFactory(typeAttribute.ExceptionType));
                continue;
            }
        }
    }

    private static Func<ServiceError, IServiceException> GetExceptionFactory(Type exceptionType)
    {
        var factory = ConstructorInvoker.Create(
            exceptionType.GetConstructor(
                BindingFlags.Public | BindingFlags.Static,
                [typeof(ServiceError)])!);

        return err => (IServiceException)factory.Invoke(err);
    }
}

[ServiceErrors]
public static class WellKnownPlatformErrors
{
    [ServiceError<PlatformException>]
    public static readonly ErrorGroupUri ErrorGroup = ErrorGroupUri.Create("platform", "errors");

    public static readonly ErrorDescriptor SomeInternalError = ErrorDescriptor.Internal(ErrorGroup, "internal-error");

    [ServiceError<PlatformException>]
    public static readonly ErrorDescriptor SomeNotFoundError = ErrorDescriptor.NotFound(ErrorGroup, "not-found");
}

public class PlatformException : ServiceException
{
    public PlatformException(ServiceError error) : base(error)
    {
    }
}
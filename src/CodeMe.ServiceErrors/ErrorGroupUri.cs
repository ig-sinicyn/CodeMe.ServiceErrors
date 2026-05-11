using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CodeMe.ServiceErrors;

/// <summary>
/// Service error group (RFC 9457-compatible problem type).
/// </summary>
public sealed class ErrorGroupUri : IEquatable<ErrorGroupUri>
{
    /// <summary>
    /// Uri-scheme for errors.
    /// </summary>
    public const string Scheme = "app-error";

    /// <summary>
    /// Error group separator.
    /// </summary>
    public const char GroupSeparator = '/';

    /// <summary>
    /// Error group string comparison.
    /// </summary>
    public const StringComparison UriPartComparison = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Creates error group.
    /// </summary>
    public static ErrorGroupUri Create(string application, string? category = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(application);
        if (application != Uri.EscapeDataString(application))
        {
            throw new ArgumentException(
                $"Application name {application} should be valid host name",
                nameof(application));
        }

        if (string.IsNullOrEmpty(category))
        {
            return new ErrorGroupUri(application, null);
        }

        var formattedCategory = new StringBuilder();
        AppendCategory(formattedCategory, category);
        return new ErrorGroupUri(application, formattedCategory.Length == 0 ? null : formattedCategory.ToString());
    }

    /// <summary>
    /// Creates error group.
    /// </summary>
    public static ErrorGroupUri Create(string application, params Span<string> categories)
    {
        ArgumentException.ThrowIfNullOrEmpty(application);
        if (application != Uri.EscapeDataString(application))
        {
            throw new ArgumentException(
                $"Application name {application} should be valid host name",
                nameof(application));
        }

        if (categories.IsEmpty)
        {
            return new ErrorGroupUri(application, null);
        }

        var formattedCategory = new StringBuilder();
        foreach (var category in categories)
        {
            AppendCategory(formattedCategory, category);
        }

        return new ErrorGroupUri(application, formattedCategory.Length == 0 ? null : formattedCategory.ToString());
    }

    /// <summary>
    /// Creates error group.
    /// </summary>
    public static ErrorGroupUri Create(string application, IEnumerable<string> categories)
    {
        var fullCategory = new StringBuilder();
        foreach (var category in categories)
        {
            AppendCategory(fullCategory, category);
        }

        return Create(application, fullCategory.ToString());
    }

    private static void AppendCategory(StringBuilder fullCategory, string category)
    {
        var categorySpan = category.AsSpan();
        var segments = categorySpan.Split(GroupSeparator);
        foreach (var segment in segments)
        {
            if (segment.Start.Equals(segment.End))
            {
                continue;
            }

            if (fullCategory.Length > 0)
            {
                fullCategory.Append(GroupSeparator);
            }

            fullCategory.Append(categorySpan[segment]);
        }
    }

    /// <summary>
    /// Creates error group from <see cref="Uri"/>.
    /// </summary>
    public static ErrorGroupUri Parse(Uri errorUri)
    {
        ArgumentNullException.ThrowIfNull(errorUri);

        if (!errorUri.IsAbsoluteUri)
        {
            throw new ArgumentException(
                $"Error group URI {errorUri} must be an absolute URI",
                nameof(errorUri));
        }

        if (!string.Equals(errorUri.Scheme, Scheme, UriPartComparison))
        {
            throw new ArgumentException(
                $"Error group URI {errorUri} scheme must be '{Scheme}'",
                nameof(errorUri));
        }

        if (errorUri.Host != errorUri.IdnHost)
        {
            throw new ArgumentException(
                $"Error group URI {errorUri} should be in IDN format",
                nameof(errorUri));
        }

        var category = errorUri.AbsolutePath;
        if (!category.EndsWith(GroupSeparator))
        {
            throw new ArgumentException(
                $"Error group URI {errorUri} must end with '{GroupSeparator}'",
                nameof(errorUri));
        }

        return Create(errorUri.Host, Uri.UnescapeDataString(category));
    }

    /// <summary>
    /// Creates error group from URI string.
    /// </summary>
    public static ErrorGroupUri Parse(string errorUri)
    {
        try
        {
            return Parse(new Uri(errorUri));
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException($"Invalid uri {errorUri} format", nameof(errorUri), ex);
        }
    }

    /// <summary>
    /// Creates error group from <see cref="Uri"/>.
    /// </summary>
    public static bool TryParse(Uri errorUri, [MaybeNullWhen(false)] out ErrorGroupUri result)
    {
        if (errorUri == null!
            || !errorUri.IsAbsoluteUri
            || !string.Equals(errorUri.Scheme, Scheme, UriPartComparison)
            || errorUri.Host != errorUri.IdnHost
            || !errorUri.AbsolutePath.EndsWith(GroupSeparator))
        {
            result = null;
            return false;
        }

        var category = Uri.UnescapeDataString(errorUri.AbsolutePath);
        result = Create(errorUri.Host, category);
        return true;
    }

    /// <summary>
    /// Creates error group from URI string.
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? errorUri,
        [MaybeNullWhen(false)] out ErrorGroupUri result)
    {
        if (!Uri.TryCreate(errorUri, UriKind.Absolute, out var uri))
        {
            result = null;
            return false;
        }

        return TryParse(uri, out result);
    }

    private static string? GetParentCategory(string? category) =>
        category.AsSpan().Trim(GroupSeparator) switch
        {
            { IsEmpty: true } => null,
            var x when x.LastIndexOf(GroupSeparator) is var index && index >= 0 => x[..index].ToString(),
            _ => null
        };

    private ErrorGroupUri(string application, string? category)
    {
        Application = application;
        Category = category;
    }

    /// <summary>
    /// Application name.
    /// </summary>
    public string Application { get; }

    /// <summary>
    /// Error category.
    /// </summary>
    public string? Category { get; }

    /// <summary>
    /// URI representation of error group.
    /// </summary>
    public Uri Uri => field ??= new UriBuilder(Scheme, Application)
    {
        Path = Category + GroupSeparator
    }.Uri;

    /// <summary>
    /// Checks if the error group has no category (is a root group).
    /// </summary>
    public bool IsApplicationGroup => Category == null;

    /// <summary>
    /// Returns the root error group.
    /// </summary>
    public ErrorGroupUri ApplicationGroup => new ErrorGroupUri(Application, null);

    /// <summary>
    /// Returns parent error group, or <c>null</c> for the root group.
    /// </summary>
    public ErrorGroupUri? ParentGroup =>
        IsApplicationGroup ? null : new ErrorGroupUri(Application, GetParentCategory(Category));

    /// <summary>
    /// Returns subgroup.
    /// </summary>
    public ErrorGroupUri SubGroup(string subCategory) =>
        Create(Application, Category ?? "", subCategory);

    /// <summary>
    /// Checks if error group contains the given subgroup.
    /// </summary>
    public bool Contains(ErrorGroupUri other)
    {
        if (!string.Equals(Application, other.Application, UriPartComparison))
        {
            return false;
        }

        if (Category == null)
        {
            return true;
        }

        if (other.Category == null)
        {
            return false;
        }

        return other.Category.StartsWith(Category, UriPartComparison)
               && (other.Category.Length == Category.Length
                   || other.Category[Category.Length] == GroupSeparator);
    }

    /// <inheritdoc/>
    public override string ToString() => Uri.ToString();

    /// <inheritdoc/>
    public bool Equals(ErrorGroupUri? other) =>
        other switch
        {
            null => false,
            _ when ReferenceEquals(this, other) => true,
            _ when string.Equals(Application, other.Application, UriPartComparison)
                   && string.Equals(Category, other.Category, UriPartComparison) => true,
            _ => false
        };

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ErrorGroupUri);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        StringComparer.OrdinalIgnoreCase.GetHashCode(Application),
        StringComparer.OrdinalIgnoreCase.GetHashCode(Category ?? ""));
}
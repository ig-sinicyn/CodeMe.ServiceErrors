using System.Diagnostics.CodeAnalysis;

namespace CodeMe.ServiceErrors;

/// <summary>
/// Service error code (RFC 9457-compatible problem type).
/// </summary>
public sealed class ErrorUri : IEquatable<ErrorUri>
{
    /// <summary>
    /// Creates error code.
    /// </summary>
    public static ErrorUri Create(ErrorGroupUri group, string code)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentException.ThrowIfNullOrEmpty(code);
        if (code.Contains(ErrorGroupUri.GroupSeparator, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Error code {code} should not contain '{ErrorGroupUri.GroupSeparator}'",
                nameof(code));
        }

        return new ErrorUri(group, code);
    }

    /// <summary>
    /// Creates error code from <see cref="Uri"/>.
    /// </summary>
    public static ErrorUri Parse(Uri errorUri)
    {
        ArgumentNullException.ThrowIfNull(errorUri);

        if (!errorUri.IsAbsoluteUri)
        {
            throw new ArgumentException(
                $"Error group URI {errorUri} must be an absolute URI",
                nameof(errorUri));
        }

        SplitUriPath(errorUri.AbsolutePath, out var category, out var errorCode);
        var group = ErrorGroupUri.Parse(
            new UriBuilder(errorUri.Scheme, errorUri.Host)
            {
                Path = category.ToString()
            }.Uri);

        if (errorCode.IsEmpty)
        {
            throw new ArgumentException(
                $"Error URI {errorUri} must not end with '{ErrorGroupUri.GroupSeparator}'",
                nameof(errorUri));
        }

        return Create(group, Uri.UnescapeDataString(errorCode));
    }

    /// <summary>
    /// Creates error code from URI string.
    /// </summary>
    public static ErrorUri Parse(string errorUri)
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
    /// Creates error code from <see cref="Uri"/>.
    /// </summary>
    public static bool TryParse(Uri errorUri, [MaybeNullWhen(false)] out ErrorUri result)
    {
        if (errorUri == null! || !errorUri.IsAbsoluteUri)
        {
            result = null;
            return false;
        }

        SplitUriPath(errorUri.AbsolutePath, out var category, out var errorCode);
        if (errorCode.IsEmpty
            || !ErrorGroupUri.TryParse(
                new UriBuilder(errorUri.Scheme, errorUri.Host)
                {
                    Path = category.ToString()
                }.Uri,
                out var group))
        {
            result = null;
            return false;
        }

        result = Create(group, Uri.UnescapeDataString(errorCode));
        return true;
    }

    /// <summary>
    /// Creates error code from URI string.
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? errorUri,
        [MaybeNullWhen(false)] out ErrorUri result)
    {
        if (!Uri.TryCreate(errorUri, UriKind.Absolute, out var uri))
        {
            result = null;
            return false;
        }

        return TryParse(uri, out result);
    }

    private static void SplitUriPath(
        string path,
        out ReadOnlySpan<char> category,
        out ReadOnlySpan<char> errorCode)
    {
        var pathSpan = path.AsSpan();
        var separatorIndex = pathSpan.LastIndexOf(ErrorGroupUri.GroupSeparator);
        category = separatorIndex >= 0
            ? pathSpan[..(separatorIndex + 1)]
            : ReadOnlySpan<char>.Empty;
        errorCode = separatorIndex >= 0
            ? pathSpan[(separatorIndex + 1)..]
            : pathSpan;
    }

    private ErrorUri(ErrorGroupUri group, string code)
    {
        Group = group;
        Code = code;
    }

    /// <summary>
    /// Error group.
    /// </summary>
    public ErrorGroupUri Group { get; }

    /// <summary>
    /// Short error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// URI representation of the error code.
    /// </summary>
    public Uri Uri => field ??= new Uri(Group.Uri, Code);

    /// <inheritdoc/>
    public override string ToString() => Uri.ToString();

    /// <inheritdoc/>
    public bool Equals(ErrorUri? other) =>
        other switch
        {
            null => false,
            _ when ReferenceEquals(this, other) => true,
            _ when Group.Equals(other.Group)
                   && string.Equals(Code, other.Code, ErrorGroupUri.UriPartComparison) => true,
            _ => false
        };

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ErrorUri);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        Group.GetHashCode(),
        StringComparer.OrdinalIgnoreCase.GetHashCode(Code));
}
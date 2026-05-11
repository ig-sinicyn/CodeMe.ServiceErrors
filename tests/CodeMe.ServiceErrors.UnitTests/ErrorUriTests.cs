namespace CodeMe.ServiceErrors.UnitTests;

public class ErrorUriTests
{
    [Fact]
    public void RootGroupError_Properties_ShouldBeExpected()
    {
        // Arrange
        var rootGroup = ErrorGroupUri.Create("service");
        var error = ErrorUri.Create(rootGroup, "error");
        var upperCaseError = ErrorUri.Create(ErrorGroupUri.Create("SERVICE"), "ERROR");

        // Assert
        error.Group.Should().Be(rootGroup);
        error.Code.Should().Be("error");
        error.Uri.ToString().Should().Be("app-error://service/error");
        error.ToString().Should().Be("app-error://service/error");

        error.GetHashCode().Should().Be(upperCaseError.GetHashCode());
        error.Equals(upperCaseError).Should().BeTrue();
    }

    [Fact]
    public void Error_Properties_ShouldBeExpected()
    {
        // Arrange
        var errorGroup = ErrorGroupUri.Create("service", "group");
        var error = ErrorUri.Create(errorGroup, "error");
        var upperCaseError = ErrorUri.Create(ErrorGroupUri.Create("SERVICE", "GROUP"), "ERROR");

        // Assert
        error.Group.Should().Be(errorGroup);
        error.Code.Should().Be("error");
        error.Uri.ToString().Should().Be("app-error://service/group/error");
        error.ToString().Should().Be("app-error://service/group/error");

        error.GetHashCode().Should().Be(upperCaseError.GetHashCode());
        error.Equals(upperCaseError).Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldBeExpected()
    {
        // Arrange
        var rootGroup = ErrorGroupUri.Create("service");
        var errorGroup = ErrorGroupUri.Create("service", "group");

        // Assert
        ErrorUri.Create(rootGroup, "error").ToString()
            .Should().Be("app-error://service/error");

        ErrorUri.Create(errorGroup, "error2").ToString()
            .Should().Be("app-error://service/group/error2");

        ErrorUri.Create(rootGroup, "👋").ToString()
            .Should().Be("app-error://service/👋");

        ErrorUri.Create(rootGroup, "-").ToString()
            .Should().Be("app-error://service/-");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a/")]
    [InlineData("a/b")]
    [InlineData("/")]
    public void Create_Bad_ShouldThrow(string? application)
    {
        // Arrange
        var errorGroup = ErrorGroupUri.Create("service", "group");

        // Assert
        if (application == null)
        {
            Assert.Throws<ArgumentNullException>(() => ErrorUri.Create(errorGroup, application!));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => ErrorUri.Create(errorGroup, application));
        }
    }

    [Fact]
    public void Parse_ShouldBeExpected()
    {
        // Assert
        ErrorUri.Parse("app-error://service/error")
            .Should().Be(ErrorUri.Create(ErrorGroupUri.Create("service"), "error"));

        ErrorUri.Parse("app-error://service/group/error")
            .Should().Be(ErrorUri.Create(ErrorGroupUri.Create("service", "group"), "error"));

        ErrorUri.Parse("app-error://service/groupA/groupB/error")
            .Should().Be(ErrorUri.Create(ErrorGroupUri.Create("service", "groupA/groupB"), "error"));

        ErrorUri.Parse("app-error://service/👋")
            .Should().Be(ErrorUri.Create(ErrorGroupUri.Create("service"), "👋"));
    }

    [Fact]
    public void TryParse_ShouldBeExpected()
    {
        // Assert
        ErrorUri.TryParse("app-error://service/error", out var result)
            .Should().BeTrue();
        result.Should().Be(ErrorUri.Create(ErrorGroupUri.Create("service"), "error"));

        ErrorUri.TryParse("app-error://service/group/error", out result)
            .Should().BeTrue();
        result.Should().Be(ErrorUri.Create(ErrorGroupUri.Create("service", "group"), "error"));

        ErrorUri.TryParse("app-error://service/groupA/groupB/error", out result)
            .Should().BeTrue();
        result.Should().Be(ErrorUri.Create(ErrorGroupUri.Create("service", "groupA/groupB"), "error"));

        ErrorUri.TryParse("app-error://service/👋", out result)
            .Should().BeTrue();
        result.Should().Be(ErrorUri.Create(ErrorGroupUri.Create("service"), "👋"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("service")]
    [InlineData("1://service")]
    [InlineData("http://service/")]
    [InlineData("app-error://service/")]
    [InlineData("app-error://👋/")]
    [InlineData("app-error://service/group/")]
    public void Parse_Bad_ShouldThrow(string? errorUri)
    {
        if (errorUri == null)
        {
            Assert.Throws<ArgumentNullException>(() => ErrorUri.Parse(errorUri!));
            Assert.Throws<ArgumentNullException>(() => ErrorUri.Parse((Uri)null!));
        }
        else if (Uri.TryCreate(errorUri, UriKind.RelativeOrAbsolute, out var uri))
        {
            Assert.Throws<ArgumentException>(() => ErrorUri.Parse(errorUri));
            Assert.Throws<ArgumentException>(() => ErrorUri.Parse(uri));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => ErrorUri.Parse(errorUri));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("service")]
    [InlineData("1://service")]
    [InlineData("http://service/")]
    [InlineData("app-error://service/")]
    [InlineData("app-error://👋/")]
    [InlineData("app-error://service/group/")]
    public void TryParse_Bad_ShouldBeFalse(string? errorUri)
    {
        ErrorUri.TryParse(errorUri, out var result).Should().BeFalse();
        result.Should().BeNull();

        if (errorUri == null)
        {
            ErrorUri.TryParse((Uri)null!, out result).Should().BeFalse();
            result.Should().BeNull();
        }
        else if (Uri.TryCreate(errorUri, UriKind.RelativeOrAbsolute, out var uri))
        {
            ErrorUri.TryParse(uri, out result).Should().BeFalse();
            result.Should().BeNull();
        }
    }
}

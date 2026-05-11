namespace CodeMe.ServiceErrors.UnitTests;

public class ErrorGroupUriTests
{
    [Fact]
    public void RootGroup_Properties_ShouldBeExpected()
    {
        // Arrange
        var rootGroup = ErrorGroupUri.Create("SERVICE");
        var errorGroup = ErrorGroupUri.Create("service", "group");
        var errorGroup2 = ErrorGroupUri.Create("service", "group2");
        var childGroup = ErrorGroupUri.Create("Service", "Group", "SubGroup");
        var lowerCaseGroup = ErrorGroupUri.Create("service");

        // Assert
        rootGroup.Application.Should().Be("SERVICE");
        rootGroup.Category.Should().BeNull();
        rootGroup.Uri.ToString().Should().Be("app-error://service/");
        rootGroup.ToString().Should().Be("app-error://service/");
        rootGroup.IsApplicationGroup.Should().BeTrue();
        rootGroup.ApplicationGroup.Should().Be(rootGroup);
        rootGroup.ParentGroup.Should().BeNull();

        rootGroup.Contains(rootGroup).Should().BeTrue();
        rootGroup.Contains(errorGroup2).Should().BeTrue();
        rootGroup.Contains(childGroup).Should().BeTrue();
        rootGroup.SubGroup("GROUP").Should().Be(errorGroup);

        rootGroup.GetHashCode().Should().Be(lowerCaseGroup.GetHashCode());
        rootGroup.Equals(lowerCaseGroup).Should().BeTrue();
    }

    [Fact]
    public void ErrorGroup_Properties_ShouldBeExpected()
    {
        // Arrange
        var rootGroup = ErrorGroupUri.Create("SERVICE");
        var errorGroup = ErrorGroupUri.Create("service", "group");
        var errorGroup2 = ErrorGroupUri.Create("service", "group2");
        var childGroup = ErrorGroupUri.Create("Service", "Group", "SubGroup");
        var upperCaseErrorGroup = ErrorGroupUri.Create("SERVICE", "GROUP");

        // Assert
        errorGroup.Application.Should().Be("service");
        errorGroup.Category.Should().Be("group");
        errorGroup.Uri.ToString().Should().Be("app-error://service/group/");
        errorGroup.ToString().Should().Be("app-error://service/group/");
        errorGroup.IsApplicationGroup.Should().BeFalse();
        errorGroup.ApplicationGroup.Should().Be(rootGroup);
        errorGroup.ParentGroup.Should().Be(rootGroup);

        errorGroup.Contains(rootGroup).Should().BeFalse();
        errorGroup.Contains(errorGroup2).Should().BeFalse();
        errorGroup.Contains(childGroup).Should().BeTrue();
        errorGroup.SubGroup("subgroup").Should().Be(childGroup);

        errorGroup.GetHashCode().Should().Be(upperCaseErrorGroup.GetHashCode());
        errorGroup.Equals(upperCaseErrorGroup).Should().BeTrue();
    }

    [Fact]
    public void ChildGroup_Properties_ShouldBeExpected()
    {
        // Arrange
        var rootGroup = ErrorGroupUri.Create("SERVICE");
        var errorGroup = ErrorGroupUri.Create("service", "group");
        var errorGroup2 = ErrorGroupUri.Create("service", "group2");
        var childGroup = ErrorGroupUri.Create("Service", "Group", "SubGroup");
        var upperCaseChildGroup = ErrorGroupUri.Create("SERVICE", "GROUP", "SUBGROUP");

        // Assert
        childGroup.Application.Should().Be("Service");
        childGroup.Category.Should().Be("Group/SubGroup");
        childGroup.Uri.ToString().Should().Be("app-error://service/Group/SubGroup/");
        childGroup.ToString().Should().Be("app-error://service/Group/SubGroup/");
        childGroup.IsApplicationGroup.Should().BeFalse();
        childGroup.ApplicationGroup.Should().Be(rootGroup);
        childGroup.ParentGroup.Should().Be(errorGroup);

        childGroup.Contains(rootGroup).Should().BeFalse();
        childGroup.Contains(errorGroup2).Should().BeFalse();
        childGroup.Contains(childGroup).Should().BeTrue();

        childGroup.GetHashCode().Should().Be(upperCaseChildGroup.GetHashCode());
        childGroup.Equals(upperCaseChildGroup).Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldBeExpected()
    {
        // Assert
        ErrorGroupUri.Create("service").ToString()
            .Should().Be("app-error://service/");

        ErrorGroupUri.Create("service", "group").ToString()
            .Should().Be("app-error://service/group/");

        ErrorGroupUri.Create("service", "groupA/groupB").ToString()
            .Should().Be("app-error://service/groupA/groupB/");

        ErrorGroupUri.Create("service", "/groupA/groupB/").ToString()
            .Should().Be("app-error://service/groupA/groupB/");

        ErrorGroupUri.Create("service", "/groupA/", "/groupB/").ToString()
            .Should().Be("app-error://service/groupA/groupB/");

        ErrorGroupUri.Create("service", "////groupA////groupB////").ToString()
            .Should().Be("app-error://service/groupA/groupB/");

        ErrorGroupUri.Create("service", "///groupA/", "groupB///").ToString()
            .Should().Be("app-error://service/groupA/groupB/");

        ErrorGroupUri.Create("service", "", "/groupA/", "", "groupB").ToString()
            .Should().Be("app-error://service/groupA/groupB/");

        ErrorGroupUri.Create("service", "👋").ToString()
            .Should().Be("app-error://service/👋/");

        ErrorGroupUri.Create("service", "-").ToString()
            .Should().Be("app-error://service/-/");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("👋")]
    public void Create_Bad_ShouldThrow(string? application)
    {
        // Assert
        if (application == null)
        {
            Assert.Throws<ArgumentNullException>(() => ErrorGroupUri.Create(application!));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => ErrorGroupUri.Create(application));
        }
    }

    [Fact]
    public void Parse_ShouldBeExpected()
    {
        // Assert
        ErrorGroupUri.Parse("app-error://service")
            .Should().Be(ErrorGroupUri.Create("service"));

        ErrorGroupUri.Parse("app-error://service/")
            .Should().Be(ErrorGroupUri.Create("service"));

        ErrorGroupUri.Parse("app-error://service/group/")
            .Should().Be(ErrorGroupUri.Create("service", "group"));

        ErrorGroupUri.Parse("app-error://service/groupA/groupB/")
            .Should().Be(ErrorGroupUri.Create("service", "groupA/groupB"));

        ErrorGroupUri.Parse("app-error://service////groupA////groupB///")
            .Should().Be(ErrorGroupUri.Create("service", "groupA/groupB"));

        ErrorGroupUri.Parse("app-error://service/👋/")
            .Should().Be(ErrorGroupUri.Create("service", "👋"));
    }

    [Fact]
    public void TryParse_ShouldBeExpected()
    {
        // Assert
        ErrorGroupUri.TryParse("app-error://service", out var result)
            .Should().BeTrue();
        result.Should().Be(ErrorGroupUri.Create("service"));

        ErrorGroupUri.TryParse("app-error://service/", out result)
            .Should().BeTrue();
        result.Should().Be(ErrorGroupUri.Create("service"));

        ErrorGroupUri.TryParse("app-error://service/group/", out result)
            .Should().BeTrue();
        result.Should().Be(ErrorGroupUri.Create("service", "group"));

        ErrorGroupUri.TryParse("app-error://service/groupA/groupB/", out result)
            .Should().BeTrue();
        result.Should().Be(ErrorGroupUri.Create("service", "groupA/groupB"));

        ErrorGroupUri.TryParse("app-error://service////groupA////groupB///", out result)
            .Should().BeTrue();
        result.Should().Be(ErrorGroupUri.Create("service", "groupA/groupB"));

        ErrorGroupUri.TryParse("app-error://service/👋/", out result)
            .Should().BeTrue();
        result.Should().Be(ErrorGroupUri.Create("service", "👋"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("service")]
    [InlineData("1://service")]
    [InlineData("http://service/")]
    [InlineData("app-error://👋/")]
    [InlineData("app-error://service/group")]
    public void Parse_Bad_ShouldThrow(string? errorUri)
    {
        if (errorUri == null)
        {
            Assert.Throws<ArgumentNullException>(() => ErrorGroupUri.Parse(errorUri!));
            Assert.Throws<ArgumentNullException>(() => ErrorGroupUri.Parse((Uri)null!));
        }
        else if (Uri.TryCreate(errorUri, UriKind.RelativeOrAbsolute, out var uri))
        {
            Assert.Throws<ArgumentException>(() => ErrorGroupUri.Parse(errorUri));
            Assert.Throws<ArgumentException>(() => ErrorGroupUri.Parse(uri));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => ErrorGroupUri.Parse(errorUri));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("service")]
    [InlineData("1://service")]
    [InlineData("http://service/")]
    [InlineData("app-error://👋/")]
    [InlineData("app-error://service/group")]
    public void TryParse_Bad_ShouldBeFalse(string? errorUri)
    {
        ErrorGroupUri.TryParse(errorUri, out var result).Should().BeFalse();
        result.Should().BeNull();

        if (errorUri == null)
        {
            ErrorGroupUri.TryParse((Uri)null!, out result).Should().BeFalse();
            result.Should().BeNull();
        }
        else if (Uri.TryCreate(errorUri, UriKind.RelativeOrAbsolute, out var uri))
        {
            ErrorGroupUri.TryParse(uri, out result).Should().BeFalse();
            result.Should().BeNull();
        }
    }
}

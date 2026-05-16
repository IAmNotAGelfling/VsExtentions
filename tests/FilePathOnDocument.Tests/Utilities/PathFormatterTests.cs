using AwesomeAssertions;
using FilePathOnDocument.Core;
using FilePathOnDocument.Utilities;
using Xunit;

namespace FilePathOnDocument.Tests.Utilities;

public class PathFormatterTests
{
    [Theory]
    [InlineData(@"C:\Projects\MyApp\src\Program.cs", @"C:\Projects\MyApp\src\Program.cs")]
    [InlineData(@"D:\Code\Test.txt", @"D:\Code\Test.txt")]
    [InlineData("", "")]
    public void GetPath_AbsolutePath_ReturnsFullPath(string input, string expected)
    {
        // Arrange & Act
        string result = PathFormatter.GetPath(input, DirectorySeparatorOption.Default, PathDisplayOption.Absolute, 2, false);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(@"C:\Projects\MyApp\src\Program.cs", 1, @"Program.cs")]
    [InlineData(@"C:\Projects\MyApp\src\Program.cs", 2, @"src\Program.cs")]
    [InlineData(@"C:\Projects\MyApp\src\Program.cs", 3, @"MyApp\src\Program.cs")]
    [InlineData(@"C:\Projects\MyApp\src\Program.cs", 10, @"C:\Projects\MyApp\src\Program.cs")]  // More than available
    [InlineData(@"C:\Single.txt", 1, @"C:\Single.txt")]
    [InlineData(@"C:\Single.txt", 5, @"C:\Single.txt")]
    public void GetTrailingPath_VariousSegmentCounts_ReturnsCorrectTrailingSegments(string input, int segments, string expected)
    {
        // Arrange & Act
        string result = PathFormatter.GetTrailingPath(input, segments);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetTrailingPath_EmptyString_ReturnsEmpty()
    {
        // Arrange & Act
        string result = PathFormatter.GetTrailingPath("", 2);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTrailingPath_NullString_ReturnsEmpty()
    {
        // Arrange & Act
        string result = PathFormatter.GetTrailingPath(null!, 2);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(DirectorySeparatorOption.Backslash, "\\")]
    [InlineData(DirectorySeparatorOption.Slash, "/")]
    [InlineData(DirectorySeparatorOption.GreaterThan, ">")]
    [InlineData(DirectorySeparatorOption.LessThan, "<")]
    [InlineData(DirectorySeparatorOption.Hyphen, "-")]
    [InlineData(DirectorySeparatorOption.Colon, ":")]
    public void GetSeparatorString_AllOptions_ReturnsCorrectSeparator(DirectorySeparatorOption option, string expected)
    {
        // Arrange & Act
        string result = PathFormatter.GetSeparatorString(option);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetSeparatorString_DefaultOption_ReturnsSystemSeparator()
    {
        // Arrange & Act
        string result = PathFormatter.GetSeparatorString(DirectorySeparatorOption.Default);

        // Assert
        result.Should().Be(System.IO.Path.DirectorySeparatorChar.ToString());
    }

    [Theory]
    [InlineData(@"C:\Projects\MyApp\src\Program.cs", DirectorySeparatorOption.Slash, @"C:/Projects/MyApp/src/Program.cs")]
    [InlineData(@"C:\Projects\MyApp\src\Program.cs", DirectorySeparatorOption.GreaterThan, @"C:>Projects>MyApp>src>Program.cs")]
    [InlineData(@"C:\Projects\MyApp\src\Program.cs", DirectorySeparatorOption.Hyphen, @"C:-Projects-MyApp-src-Program.cs")]
    public void GetPath_CustomSeparator_ReplacesSeparator(string input, DirectorySeparatorOption separator, string expected)
    {
        // Arrange & Act
        string result = PathFormatter.GetPath(input, separator, PathDisplayOption.Absolute, 2, false);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetPath_CustomSeparatorWithSpaces_AddsSpacesAroundSeparator()
    {
        // Arrange
        string input = @"C:\Projects\MyApp\src\Program.cs";

        // Act
        string result = PathFormatter.GetPath(input, DirectorySeparatorOption.GreaterThan, PathDisplayOption.Absolute, 2, true);

        // Assert
        result.Should().Be(@"C: > Projects > MyApp > src > Program.cs");
    }

    [Fact]
    public void GetPath_TrailingPathWithCustomSeparator_CombinesBothOptions()
    {
        // Arrange
        string input = @"C:\Projects\MyApp\src\Program.cs";

        // Act
        string result = PathFormatter.GetPath(input, DirectorySeparatorOption.Slash, PathDisplayOption.TrailingPath, 2, false);

        // Assert
        result.Should().Be(@"src/Program.cs");
    }

    [Theory]
    [InlineData("Short", 10, "Short")]
    [InlineData("ExactlyTen", 10, "ExactlyTen")]
    [InlineData("ThisIsAVeryLongPathThatNeedsTrimming", 20, "…thThatNeedsTrimming")]
    [InlineData("", 10, "")]
    public void TrimPathFromStart_VariousLengths_TrimsCorrectly(string input, int maxLength, string expected)
    {
        // Arrange & Act
        string result = PathFormatter.TrimPathFromStart(input, maxLength);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TrimPathFromStart_NullInput_ReturnsEmpty()
    {
        // Arrange & Act
        string result = PathFormatter.TrimPathFromStart(null!, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(100, 8.0, 20)]  // (100-20)/8 = 10, min is 20
    [InlineData(400, 8.0, 47)]  // (400-20)/8 = 47.5
    [InlineData(1000, 8.0, 100)] // (1000-20)/8 = 122.5, max is 100
    [InlineData(50, 8.0, 20)]   // (50-20)/8 = 3.75, min is 20
    public void CalculateMaxCharacters_VariousWidths_ReturnsWithinBounds(double maxWidth, double charWidth, int expected)
    {
        // Arrange & Act
        int result = PathFormatter.CalculateMaxCharacters(maxWidth, charWidth);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateMaxCharacters_DefaultCharWidth_UsesDefaultValue()
    {
        // Arrange & Act
        int result = PathFormatter.CalculateMaxCharacters(400);

        // Assert
        result.Should().Be(47); // (400-20)/8.0
    }

    [Theory]
    [InlineData(@"C:\Folder1\Folder2\File.txt", DirectorySeparatorOption.Colon, PathDisplayOption.TrailingPath, 2, true, @"Folder2 : File.txt")]
    [InlineData(@"D:\A\B\C\D\E\F.cs", DirectorySeparatorOption.GreaterThan, PathDisplayOption.TrailingPath, 3, false, @"D>E>F.cs")]
    public void GetPath_ComplexScenarios_ProducesExpectedOutput(string input, DirectorySeparatorOption separator, PathDisplayOption pathOption, int trailingLevel, bool spaceAround, string expected)
    {
        // Arrange & Act
        string result = PathFormatter.GetPath(input, separator, pathOption, trailingLevel, spaceAround);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetPath_NullInput_ReturnsEmpty()
    {
        // Arrange & Act
        string result = PathFormatter.GetPath(null!, DirectorySeparatorOption.Default, PathDisplayOption.Absolute, 2, false);

        // Assert
        result.Should().BeEmpty();
    }
}

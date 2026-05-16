using AwesomeAssertions;

namespace FilePathOnDocument.Tests;

/// <summary>
/// Tests for path formatting logic.
/// TODO: Add tests once extraction of formatting logic from FilePathMargin is complete.
/// </summary>
public class PathFormattingTests
{
    [Fact]
    public void Placeholder_Test()
    {
        // Arrange
        bool expected = true;

        // Act
        bool actual = true;

        // Assert
        actual.Should().Be(expected);
    }
}

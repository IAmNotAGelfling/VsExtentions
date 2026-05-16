using AwesomeAssertions;
using FilePathOnDocument.Utilities;
using System;
using System.IO;
using Xunit;

namespace FilePathOnDocument.Tests.Utilities;

public class InternalPathDetectorTests
{
    [Fact]
    public void IsInternalPath_NullPath_ReturnsTrue()
    {
        // Arrange
        string[] internalPaths = ["Temp"];

        // Act
        bool result = InternalPathDetector.IsInternalPath(null, internalPaths);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternalPath_EmptyPath_ReturnsTrue()
    {
        // Arrange
        string[] internalPaths = ["Temp"];

        // Act
        bool result = InternalPathDetector.IsInternalPath("", internalPaths);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternalPath_WhitespacePath_ReturnsTrue()
    {
        // Arrange
        string[] internalPaths = ["Temp"];

        // Act
        bool result = InternalPathDetector.IsInternalPath("   ", internalPaths);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternalPath_PathInTempFolder_ReturnsTrue()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string testPath = Path.Combine(tempPath, "TestFile.txt");
        string[] internalPaths = [""];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternalPath_PathInCustomInternalFolder_ReturnsTrue()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string internalFolder = "MyInternalFiles";
        string fullInternalPath = Path.Combine(tempPath, internalFolder);
        string testPath = Path.Combine(fullInternalPath, "TestFile.txt");
        string[] internalPaths = [internalFolder];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternalPath_PathNotInInternalFolders_ReturnsFalse()
    {
        // Arrange
        string testPath = @"C:\Projects\MyApp\Program.cs";
        string[] internalPaths = ["Temp", "TempFiles"];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInternalPath_CaseInsensitiveMatch_ReturnsTrue()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string internalFolder = "MyInternalFiles";
        string fullInternalPath = Path.Combine(tempPath, internalFolder);
        string testPath = Path.Combine(fullInternalPath, "TestFile.txt").ToUpperInvariant();
        string[] internalPaths = [internalFolder.ToLowerInvariant()];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternalPath_MultipleInternalPaths_MatchesAny()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string internalFolder1 = "Internal1";
        string internalFolder2 = "Internal2";
        string fullInternalPath2 = Path.Combine(tempPath, internalFolder2);
        string testPath = Path.Combine(fullInternalPath2, "TestFile.txt");
        string[] internalPaths = [internalFolder1, internalFolder2];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternalPath_EmptyInternalPathsList_OnlyMatchesTempRoot()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string testPath = Path.Combine(tempPath, "DirectFile.txt");
        string[] internalPaths = [];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeFalse(); // Empty array means no internal paths to match
    }

    [Fact]
    public void IsInternalPath_NestedInternalPath_ReturnsTrue()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string internalFolder = @"Level1\Level2\Level3";
        string fullInternalPath = Path.GetFullPath(Path.Combine(tempPath, internalFolder));
        string testPath = Path.Combine(fullInternalPath, "Deep", "File.txt");
        string[] internalPaths = [internalFolder];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternalPath_PathContainsButNotWithinInternal_ReturnsFalse()
    {
        // Arrange
        string testPath = @"C:\MyTemp\SomeFolder\File.txt"; // Contains "Temp" but not in system temp
        string[] internalPaths = ["Temp"];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInternalPath_WithBackslashesAndForwardSlashes_HandlesCorrectly()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string internalFolder = "TestFolder/SubFolder";
        string normalizedFolder = internalFolder.Replace('/', Path.DirectorySeparatorChar);
        string fullInternalPath = Path.GetFullPath(Path.Combine(tempPath, normalizedFolder));
        string testPath = Path.Combine(fullInternalPath, "File.txt");
        string[] internalPaths = [normalizedFolder];

        // Act
        bool result = InternalPathDetector.IsInternalPath(testPath, internalPaths);

        // Assert
        result.Should().BeTrue();
    }
}

using AwesomeAssertions;
using FilePathOnDocument.Utilities;
using System;
using System.IO;
using Xunit;

namespace FilePathOnDocument.Tests.Utilities;

public class PathResolverTests
{
    private readonly string _tempTestRoot;

    public PathResolverTests()
    {
        _tempTestRoot = Path.Combine(Path.GetTempPath(), "PathResolverTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempTestRoot);
    }

    [Fact]
    public void FindProjectDirectory_NullPath_ReturnsNull()
    {
        // Arrange & Act
        string? result = PathResolver.FindProjectDirectory(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindProjectDirectory_EmptyPath_ReturnsNull()
    {
        // Arrange & Act
        string? result = PathResolver.FindProjectDirectory("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindProjectDirectory_WithCsProj_FindsProjectDirectory()
    {
        // Arrange
        string projectDir = Path.Combine(_tempTestRoot, "MyProject");
        string srcDir = Path.Combine(projectDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(Path.Combine(projectDir, "MyProject.csproj"), "<Project />");
        string testFile = Path.Combine(srcDir, "Program.cs");

        // Act
        string? result = PathResolver.FindProjectDirectory(testFile);

        // Assert
        result.Should().Be(projectDir);

        // Cleanup
        Directory.Delete(_tempTestRoot, true);
    }

    [Fact]
    public void FindProjectDirectory_WithVbProj_FindsProjectDirectory()
    {
        // Arrange
        string projectDir = Path.Combine(_tempTestRoot, "MyVbProject");
        string srcDir = Path.Combine(projectDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(Path.Combine(projectDir, "MyVbProject.vbproj"), "<Project />");
        string testFile = Path.Combine(srcDir, "Module1.vb");

        // Act
        string? result = PathResolver.FindProjectDirectory(testFile);

        // Assert
        result.Should().Be(projectDir);

        // Cleanup
        Directory.Delete(_tempTestRoot, true);
    }

    [Fact]
    public void FindProjectDirectory_WithFsProj_FindsProjectDirectory()
    {
        // Arrange
        string projectDir = Path.Combine(_tempTestRoot, "MyFsProject");
        string srcDir = Path.Combine(projectDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(Path.Combine(projectDir, "MyFsProject.fsproj"), "<Project />");
        string testFile = Path.Combine(srcDir, "Program.fs");

        // Act
        string? result = PathResolver.FindProjectDirectory(testFile);

        // Assert
        result.Should().Be(projectDir);

        // Cleanup
        Directory.Delete(_tempTestRoot, true);
    }

    [Fact]
    public void FindProjectDirectory_NoProjectFile_ReturnsNull()
    {
        // Arrange
        string srcDir = Path.Combine(_tempTestRoot, "src");
        Directory.CreateDirectory(srcDir);
        string testFile = Path.Combine(srcDir, "Program.cs");

        // Act
        string? result = PathResolver.FindProjectDirectory(testFile);

        // Assert
        result.Should().BeNull();

        // Cleanup
        Directory.Delete(_tempTestRoot, true);
    }

    [Fact]
    public void FindSolutionDirectory_NullPath_ReturnsNull()
    {
        // Arrange & Act
        string? result = PathResolver.FindSolutionDirectory(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindSolutionDirectory_WithSlnFile_FindsSolutionDirectory()
    {
        // Arrange
        string solutionDir = Path.Combine(_tempTestRoot, "MySolution");
        string projectDir = Path.Combine(solutionDir, "MyProject");
        string srcDir = Path.Combine(projectDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(Path.Combine(solutionDir, "MySolution.sln"), "");
        string testFile = Path.Combine(srcDir, "Program.cs");

        // Act
        string? result = PathResolver.FindSolutionDirectory(testFile);

        // Assert
        result.Should().Be(solutionDir);

        // Cleanup
        Directory.Delete(_tempTestRoot, true);
    }

    [Fact]
    public void FindSolutionDirectory_WithSlnxFile_FindsSolutionDirectory()
    {
        // Arrange
        string solutionDir = Path.Combine(_tempTestRoot, "MySolution");
        string projectDir = Path.Combine(solutionDir, "MyProject");
        string srcDir = Path.Combine(projectDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(Path.Combine(solutionDir, "MySolution.slnx"), "<Solution />");
        string testFile = Path.Combine(srcDir, "Program.cs");

        // Act
        string? result = PathResolver.FindSolutionDirectory(testFile);

        // Assert
        result.Should().Be(solutionDir);

        // Cleanup
        Directory.Delete(_tempTestRoot, true);
    }

    [Fact]
    public void FindSolutionDirectory_NoSolutionFile_ReturnsNull()
    {
        // Arrange
        string srcDir = Path.Combine(_tempTestRoot, "src");
        Directory.CreateDirectory(srcDir);
        string testFile = Path.Combine(srcDir, "Program.cs");

        // Act
        string? result = PathResolver.FindSolutionDirectory(testFile);

        // Assert
        result.Should().BeNull();

        // Cleanup
        Directory.Delete(_tempTestRoot, true);
    }

    [Theory]
    [InlineData(@"C:\Solution\Project\src\Program.cs", @"C:\Solution\Project", @"src\Program.cs")]
    [InlineData(@"C:\Solution\Project\Program.cs", @"C:\Solution\Project", "Program.cs")]
    [InlineData(@"C:\Solution\Project\src\folder\File.cs", @"C:\Solution\Project", @"src\folder\File.cs")]
    public void GetProjectRelativePath_WithinProject_ReturnsRelativePath(string filePath, string projectDir, string expected)
    {
        // Arrange & Act
        string result = PathResolver.GetProjectRelativePath(filePath, projectDir);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetProjectRelativePath_OutsideProject_ReturnsFullPath()
    {
        // Arrange
        string filePath = @"C:\Other\File.cs";
        string projectDir = @"C:\Solution\Project";

        // Act
        string result = PathResolver.GetProjectRelativePath(filePath, projectDir);

        // Assert
        result.Should().Be(filePath);
    }

    [Fact]
    public void GetProjectRelativePath_NullProjectDir_ReturnsFullPath()
    {
        // Arrange
        string filePath = @"C:\File.cs";

        // Act
        string result = PathResolver.GetProjectRelativePath(filePath, null);

        // Assert
        result.Should().Be(filePath);
    }

    [Theory]
    [InlineData(@"C:\Solution\Project\src\Program.cs", @"C:\Solution", @"Project\src\Program.cs")]
    [InlineData(@"C:\Solution\Project1\File.cs", @"C:\Solution", @"Project1\File.cs")]
    public void GetSolutionRelativePath_WithinSolution_ReturnsRelativePath(string filePath, string solutionDir, string expected)
    {
        // Arrange & Act
        string result = PathResolver.GetSolutionRelativePath(filePath, solutionDir);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetSolutionRelativePath_OutsideSolution_ReturnsFullPath()
    {
        // Arrange
        string filePath = @"C:\Other\File.cs";
        string solutionDir = @"C:\Solution";

        // Act
        string result = PathResolver.GetSolutionRelativePath(filePath, solutionDir);

        // Assert
        result.Should().Be(filePath);
    }

    [Fact]
    public void GetSolutionRelativePath_NullSolutionDir_ReturnsFullPath()
    {
        // Arrange
        string filePath = @"C:\File.cs";

        // Act
        string result = PathResolver.GetSolutionRelativePath(filePath, null);

        // Assert
        result.Should().Be(filePath);
    }

    [Fact]
    public void GetProjectRelativePath_EmptyFilePath_ReturnsEmpty()
    {
        // Arrange & Act
        string result = PathResolver.GetProjectRelativePath("", @"C:\Project");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetProjectRelativePath_CaseInsensitive_ReturnsRelativePath()
    {
        // Arrange
        string filePath = @"C:\Solution\PROJECT\src\Program.cs";
        string projectDir = @"C:\solution\project";

        // Act
        string result = PathResolver.GetProjectRelativePath(filePath, projectDir);

        // Assert
        result.Should().Be(@"src\Program.cs");
    }
}

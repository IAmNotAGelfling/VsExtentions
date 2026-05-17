using System.IO;

namespace FilePathOnDocument.Utilities;

/// <summary>
/// Provides utilities for resolving project and solution directories from file paths.
/// </summary>
public static class PathResolver
{
    /// <summary>
    /// Finds the project directory by walking up the directory tree looking for project files.
    /// </summary>
    /// <param name="filePath">The file path to start searching from.</param>
    /// <returns>The project directory path, or null if not found.</returns>
    public static string? FindProjectDirectory(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        string? dir = Path.GetDirectoryName(filePath);
        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.GetFiles(dir, "*.csproj").Length > 0 ||
                Directory.GetFiles(dir, "*.vbproj").Length > 0 ||
                Directory.GetFiles(dir, "*.fsproj").Length > 0)
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }

    /// <summary>
    /// Finds the solution directory by walking up the directory tree looking for solution files.
    /// </summary>
    /// <param name="filePath">The file path to start searching from.</param>
    /// <returns>The solution directory path, or null if not found.</returns>
    public static string? FindSolutionDirectory(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        string? dir = Path.GetDirectoryName(filePath);
        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0 ||
                Directory.GetFiles(dir, "*.slnx").Length > 0)
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }

    /// <summary>
    /// Gets a project-relative path by removing the project directory prefix.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <param name="projectDirectory">The project directory.</param>
    /// <returns>The relative path, or the original path if not within the project.</returns>
    public static string GetProjectRelativePath(string filePath, string? projectDirectory)
    {
        return GetRelativePath(filePath, projectDirectory);
    }

    /// <summary>
    /// Gets a solution-relative path by removing the solution directory prefix.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <param name="solutionDirectory">The solution directory.</param>
    /// <returns>The relative path, or the original path if not within the solution.</returns>
    public static string GetSolutionRelativePath(string filePath, string? solutionDirectory)
    {
        return GetRelativePath(filePath, solutionDirectory);
    }

    /// <summary>
    /// Gets a relative path by removing the base directory prefix.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <param name="baseDirectory">The base directory to remove.</param>
    /// <returns>The relative path, or the original path if not within the base directory.</returns>
    private static string GetRelativePath(string filePath, string? baseDirectory)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        if (string.IsNullOrEmpty(baseDirectory))
            return filePath;

        // After checking IsNullOrEmpty above, baseDirectory is guaranteed non-null here
        if (filePath.StartsWith(baseDirectory, System.StringComparison.OrdinalIgnoreCase))
        {
            return filePath.Substring(baseDirectory!.Length).TrimStart('\\', '/');
        }

        return filePath;
    }
}

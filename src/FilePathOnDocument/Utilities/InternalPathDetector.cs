using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FilePathOnDocument.Utilities;

/// <summary>
/// Provides utilities for detecting internal/temporary file paths.
/// </summary>
public static class InternalPathDetector
{
    /// <summary>
    /// Determines whether a file path is considered an internal/temporary path.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="internalPaths">Collection of internal path patterns to check against.</param>
    /// <returns>True if the path is internal, false otherwise.</returns>
    public static bool IsInternalPath(string? filePath, IEnumerable<string> internalPaths)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return true;

        string tempPath = Path.GetTempPath();

        // After the null check above, filePath is guaranteed non-null here
        foreach (string internalFile in internalPaths)
        {
            // Empty string matches temp root directly
            if (string.IsNullOrEmpty(internalFile))
            {
                if (filePath!.IndexOf(tempPath, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                continue;
            }

            // Skip whitespace-only entries
            if (string.IsNullOrWhiteSpace(internalFile))
                continue;

            string fullPath = Path.GetFullPath(Path.Combine(tempPath, internalFile));
            if (filePath!.IndexOf(fullPath, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a file path is considered an internal/temporary path.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="internalPaths">Array of internal path patterns to check against.</param>
    /// <returns>True if the path is internal, false otherwise.</returns>
    public static bool IsInternalPath(string? filePath, params string[] internalPaths)
    {
        return IsInternalPath(filePath, (IEnumerable<string>)internalPaths);
    }
}

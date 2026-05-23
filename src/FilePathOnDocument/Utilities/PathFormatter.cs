using FilePathOnDocument.Core;
using System;
using System.IO;
using System.Linq;

namespace FilePathOnDocument.Utilities;

/// <summary>
/// Provides path formatting utilities for displaying file paths in various formats.
/// </summary>
public static class PathFormatter
{
    /// <summary>
    /// Formats a full path according to the specified options.
    /// </summary>
    /// <param name="fullPath">The full path to format.</param>
    /// <param name="separator">The directory separator to use.</param>
    /// <param name="pathOption">Whether to show absolute or trailing path.</param>
    /// <param name="trailingLevel">Number of trailing segments to show (for trailing path option).</param>
    /// <param name="addSpaceAround">Whether to add spaces around the separator.</param>
    /// <returns>The formatted path string.</returns>
    public static string GetPath(string fullPath, DirectorySeparatorOption separator,
        PathDisplayOption pathOption, int trailingLevel, bool addSpaceAround)
    {
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        string path = pathOption == PathDisplayOption.TrailingPath
            ? GetTrailingPath(fullPath, trailingLevel)
            : fullPath;

        if (separator == DirectorySeparatorOption.Default)
            return path;

        string separatorChar = GetSeparatorString(separator);
        string replacement = addSpaceAround ? $" {separatorChar} " : separatorChar;

        return path.Replace(Path.DirectorySeparatorChar.ToString(), replacement);
    }

    /// <summary>
    /// Gets the string representation of a directory separator option.
    /// </summary>
    /// <param name="option">The separator option.</param>
    /// <returns>The separator character or string.</returns>
    public static string GetSeparatorString(DirectorySeparatorOption option)
    {
        return option switch
        {
            DirectorySeparatorOption.Backslash => "\\",
            DirectorySeparatorOption.Slash => "/",
            DirectorySeparatorOption.GreaterThan => ">",
            DirectorySeparatorOption.LessThan => "<",
            DirectorySeparatorOption.Hyphen => "-",
            DirectorySeparatorOption.Colon => ":",
            _ => Path.DirectorySeparatorChar.ToString()
        };
    }

    /// <summary>
    /// Gets the trailing segments of a path.
    /// </summary>
    /// <param name="fullPath">The full path.</param>
    /// <param name="segments">Number of trailing segments to include.</param>
    /// <returns>Path containing only the specified number of trailing segments.</returns>
    public static string GetTrailingPath(string fullPath, int segments)
    {
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        string[] parts = fullPath.Split(
        [
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        ], StringSplitOptions.RemoveEmptyEntries);

        // If the path has fewer or equal segments than requested, return as-is
        if (parts.Length <= segments)
            return fullPath;

        // Check if first part is a drive letter (e.g., "C:")
        bool hasDrive = parts.Length > 0 && parts[0].Length == 2 && parts[0][1] == ':';

        // If we have a drive and would end up with just the drive + file (2 parts total)
        // requesting 1 segment, return the full path to preserve the drive
        if (hasDrive && parts.Length == 2 && segments == 1)
            return fullPath;

        // Otherwise take the requested number of trailing segments
        string[] trailing = [.. parts.Skip(parts.Length - segments)];
        return Path.Combine(trailing);
    }

    /// <summary>
    /// Trims a path from the start if it exceeds the maximum length, adding an ellipsis.
    /// </summary>
    /// <param name="path">The path to trim.</param>
    /// <param name="maxLength">Maximum length allowed.</param>
    /// <returns>The trimmed path with ellipsis if needed.</returns>
    public static string TrimPathFromStart(string path, int maxLength)
    {
        if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
            return path ?? string.Empty;

        // Ellipsis takes 1 character, so we have (maxLength - 1) chars left for the path tail
        int charsToTake = maxLength - 1;
        return "…" + path.Substring(path.Length - charsToTake);
    }

    /// <summary>
    /// Calculates the maximum number of characters that can fit in a given width.
    /// </summary>
    /// <param name="maxWidth">Maximum width in pixels.</param>
    /// <param name="charWidth">Approximate width of a single character in pixels.</param>
    /// <returns>Maximum number of characters (between 20 and 100).</returns>
    public static int CalculateMaxCharacters(double maxWidth, double charWidth = 8.0)
    {
        return Math.Max(20, Math.Min((int)((maxWidth - 20) / charWidth), 100));
    }
}

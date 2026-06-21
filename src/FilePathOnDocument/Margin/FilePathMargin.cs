using FilePathOnDocument.Core;
using FilePathOnDocument.Options;
using FilePathOnDocument.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FilePathOnDocument.Margin;

internal class FilePathMargin : Canvas, IWpfTextViewMargin
{
    public const string MarginName = "FilePathOnFooter";

    private readonly DocumentMonitor _documentMonitor;
    private readonly TextBlock _lblFilePath = null!;
    private readonly AlignmentOption _alignment;
    private bool _isDisposed;

    public FilePathMargin(IWpfTextView textView, AlignmentOption alignment)
    {
        if (textView == null) throw new ArgumentNullException(nameof(textView));
        _alignment = alignment;

        _documentMonitor = new DocumentMonitor(GetDocument(textView));
        if (string.IsNullOrWhiteSpace(_documentMonitor.FileName) ||
            _documentMonitor.FileName == "Temp.txt")
        {
            return;
        }

        _documentMonitor.PropertyChanged += OnDocumentFileNameChanged;
        Loaded += OnLoaded;
        ClipToBounds = true;

        _lblFilePath = new TextBlock
        {
            Padding = _alignment == AlignmentOption.BottomControl
                ? new Thickness(0, 0, 10, 0)
                : new Thickness(0),
            ContextMenu = CreateContextMenu()
        };

        Children.Add(_lblFilePath);

        SetResourceReference(BackgroundProperty, EnvironmentColors.ScrollBarBackgroundBrushKey);
        _lblFilePath.SetResourceReference(TextBlock.ForegroundProperty, EnvironmentColors.ComboBoxTextBrushKey);
        _lblFilePath.SetResourceReference(TextBlock.BackgroundProperty, EnvironmentColors.ScrollBarBackgroundBrushKey);

        UpdateDisplay(_documentMonitor.FileName);
    }

    private static ITextDocument? GetDocument(IWpfTextView textView)
    {
        textView.TextDataModel.DocumentBuffer.Properties
            .TryGetProperty<ITextDocument>(typeof(ITextDocument), out ITextDocument? document);
        return document;
    }

    private void OnDocumentFileNameChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentMonitor.FileName))
            UpdateDisplay(_documentMonitor.FileName);
    }

    public void RefreshFromSettings()
    {
        UpdateDisplay(_documentMonitor.FileName);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_alignment == AlignmentOption.BottomControl)
        {
            _lblFilePath.MaxWidth = 400;
            _lblFilePath.MinWidth = 100;
            UpdateBottomControlSize();
        }
        else
        {
            Height = _lblFilePath.ActualHeight;
        }

        _lblFilePath.SizeChanged += (s, ev) => Height = _lblFilePath.ActualHeight;

        Loaded -= OnLoaded;
    }

    private void UpdateDisplay(string? filePath)
    {
        GeneralOptions options = GeneralOptions.Instance;

        if (options.ExtensionDisabled || ShouldHideInternalPath(filePath))
        {
            Visibility = Visibility.Collapsed;
            return;
        }

        Visibility = Visibility.Visible;
        string displayPath = FormatPath(filePath);

        if (_alignment == AlignmentOption.BottomControl)
        {
            int maxChars = CalculateMaxCharacters(400);
            _lblFilePath.Text = TrimPathFromStart(displayPath, maxChars);
            UpdateBottomControlSize();
        }
        else
        {
            _lblFilePath.Text = displayPath;
        }

        ToolTip = $"{filePath}{Environment.NewLine}" +
                  "Right-click for copy options and to open containing folder{Environment.NewLine}" +
                  "(Configure via Tools → Options → File Path On Document)";
    }

    private static bool ShouldHideInternalPath(string? filePath)
    {
        GeneralOptions options = GeneralOptions.Instance;

        if (options.ShowInternalFilePaths == ShowInternalFilePathsOption.Show)
            return false;

        if (!IsInternalPath(filePath))
            return false;

        return options.ShowInternalFilePaths == ShowInternalFilePathsOption.Hide;
    }

    private static bool IsInternalPath(string? filePath)
    {
        InternalPathsOptions internalOptions = InternalPathsOptions.Instance;
        return InternalPathDetector.IsInternalPath(filePath, internalOptions.GetPaths());
    }

    private static string FormatPath(string? fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        // After null check, fullPath is guaranteed non-null
        GeneralOptions options = GeneralOptions.Instance;

        if (IsInternalPath(fullPath) &&
            options.ShowInternalFilePaths == ShowInternalFilePathsOption.ShowFileName)
        {
            return PathFormatter.GetPath(fullPath!, options.DirectorySeparator,
                PathDisplayOption.TrailingPath, 1, options.SpaceAround);
        }

        return PathFormatter.GetPath(fullPath!, options.DirectorySeparator,
            options.PathDisplay, options.TrailingPathLevel, options.SpaceAround);
    }

    private static string TrimPathFromStart(string path, int maxLength)
    {
        return PathFormatter.TrimPathFromStart(path, maxLength);
    }

    private static int CalculateMaxCharacters(double maxWidth)
    {
        return PathFormatter.CalculateMaxCharacters(maxWidth);
    }

    private void UpdateBottomControlSize()
    {
        _lblFilePath.Measure(new Size(400, double.PositiveInfinity));
        double width = Math.Min(Math.Max(_lblFilePath.DesiredSize.Width, 100), 400);
        Width = width;
        _lblFilePath.Width = width;
        Height = _lblFilePath.DesiredSize.Height;
    }

    private void OpenContainingFolder()
    {
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{_documentMonitor.FileName}\"");
        }
        catch
        {
            // Silent failure - don't crash VS if explorer.exe fails or file doesn't exist
        }
    }

    private ContextMenu CreateContextMenu()
    {
        ContextMenu menu = new();

        MenuItem copyFullPath = new() { Header = "Copy Full Path" };
        copyFullPath.Click += (s, e) => CopyToClipboard(GetFullPath());

        MenuItem copyFileName = new() { Header = "Copy File Name" };
        copyFileName.Click += (s, e) => CopyToClipboard(GetFileName());

        MenuItem copyProjectRelative = new() { Header = "Copy Project Relative Path" };
        copyProjectRelative.Click += (s, e) => CopyToClipboard(GetProjectRelativePath());

        MenuItem copySolutionRelative = new() { Header = "Copy Solution Relative Path" };
        copySolutionRelative.Click += (s, e) => CopyToClipboard(GetSolutionRelativePath());

        menu.Items.Add(copyFullPath);
        menu.Items.Add(copyFileName);
        menu.Items.Add(copyProjectRelative);
        menu.Items.Add(copySolutionRelative);

        if (_documentMonitor.Namespace != null)
        {
            MenuItem copyNamespace = new() { Header = "Copy Namespace" };
            copyNamespace.Click += (s, e) => CopyToClipboard(_documentMonitor.Namespace ?? string.Empty);
            menu.Items.Add(copyNamespace);
        }

        menu.Items.Add(new Separator());

        MenuItem openFolder = new() { Header = "Open Containing Folder" };
        openFolder.Click += (s, e) => OpenContainingFolder();
        menu.Items.Add(openFolder);

        return menu;
    }

    private static void CopyToClipboard(string text)
    {
        try
        {
            if (!string.IsNullOrEmpty(text))
                Clipboard.SetText(text);
        }
        catch
        {
            // Silent failure - don't crash VS if namespace extraction fails
        }
    }

    private string GetFileName()
    {
        string? fullPath = _documentMonitor.FileName;
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        return System.IO.Path.GetFileName(fullPath);
    }

    private string GetFullPath()
    {
        return _documentMonitor.FileName ?? string.Empty;
    }

    private string GetProjectRelativePath()
    {
        string? fullPath = _documentMonitor.FileName;
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        return MakeRelative(fullPath!, _documentMonitor.ProjectDir) ?? fullPath!;
    }

    private string GetSolutionRelativePath()
    {
        string? fullPath = _documentMonitor.FileName;
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        return MakeRelative(fullPath!, _documentMonitor.SolutionDir) ?? fullPath!;
    }

    private static string? MakeRelative(string fullPath, string? baseDir)
    {
        if (string.IsNullOrEmpty(baseDir))
            return null;

        if (fullPath.StartsWith(baseDir!, StringComparison.OrdinalIgnoreCase))
            return fullPath.Substring(baseDir!.Length).TrimStart('\\', '/');

        return null;
    }


    public FrameworkElement VisualElement
    {
        get
        {
            ThrowIfDisposed();
            return this;
        }
    }

    public double MarginSize
    {
        get
        {
            ThrowIfDisposed();
            return ActualHeight;
        }
    }

    public bool Enabled
    {
        get
        {
            ThrowIfDisposed();
            return true;
        }
    }

    public ITextViewMargin? GetTextViewMargin(string marginName)
    {
        string topName = marginName + "Top";
        string bottomName = marginName + "Bottom";
        string bottomControlName = marginName + "BottomControl";

        return string.Equals(topName, MarginName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(bottomName, MarginName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(bottomControlName, MarginName, StringComparison.OrdinalIgnoreCase)
            ? this
            : null;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _documentMonitor?.PropertyChanged -= OnDocumentFileNameChanged;
        }

        _isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(FilePathMargin));
    }
}

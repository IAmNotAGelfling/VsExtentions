using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FilePathOnDocument.Core;
using FilePathOnDocument.Options;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FilePathOnDocument.Margin;

internal class FilePathMargin : Canvas, IWpfTextViewMargin
{
    public const string MarginName = "FilePathOnFooter";

    private static readonly Regex CSharpNamespaceRegex = new Regex(@"namespace\s+([\w\.]+)", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex VbNamespaceRegex = new Regex(@"Namespace\s+([\w\.]+)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private readonly IWpfTextView _textView;
    private readonly DocumentMonitor _documentMonitor;
    private readonly TextBox _lblFilePath = null!;
    private readonly AlignmentOption _alignment;
    private bool _isDisposed;

    public FilePathMargin(IWpfTextView textView, AlignmentOption alignment)
    {
        _textView = textView ?? throw new ArgumentNullException(nameof(textView));
        _alignment = alignment;

        _documentMonitor = new DocumentMonitor(GetDocument());
        if (string.IsNullOrWhiteSpace(_documentMonitor.FileName) ||
            _documentMonitor.FileName == "Temp.txt")
            return;

        _documentMonitor.PropertyChanged += OnDocumentFileNameChanged;
        Loaded += OnLoaded;
        ClipToBounds = true;

        _lblFilePath = new TextBox
        {
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Padding = _alignment == AlignmentOption.BottomControl
                ? new Thickness(0, 0, 10, 0)
                : new Thickness(0)
        };

        _lblFilePath.ContextMenu = CreateContextMenu();

        Children.Add(_lblFilePath);

        SetResourceReference(BackgroundProperty, EnvironmentColors.ScrollBarBackgroundBrushKey);
        _lblFilePath.SetResourceReference(TextBox.ForegroundProperty, EnvironmentColors.ComboBoxTextBrushKey);
        _lblFilePath.SetResourceReference(TextBox.BackgroundProperty, EnvironmentColors.ScrollBarBackgroundBrushKey);

        UpdateDisplay(_documentMonitor.FileName);
    }

    private ITextDocument? GetDocument()
    {
        _textView.TextDataModel.DocumentBuffer.Properties
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
        _lblFilePath.PreviewMouseDown += OnMouseDown;

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

    private bool ShouldHideInternalPath(string? filePath)
    {
        GeneralOptions options = GeneralOptions.Instance;

        if (options.ShowInternalFilePaths == ShowInternalFilePathsOption.Show)
            return false;

        if (!IsInternalPath(filePath))
            return false;

        return options.ShowInternalFilePaths == ShowInternalFilePathsOption.Hide;
    }

    private bool IsInternalPath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return true;

        string tempPath = System.IO.Path.GetTempPath();
        InternalPathsOptions internalOptions = InternalPathsOptions.Instance;

        foreach (string internalFile in internalOptions.GetPaths())
        {
            string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(tempPath, internalFile));
            if (filePath.IndexOf(fullPath, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    private string FormatPath(string? fullPath)
    {
        GeneralOptions options = GeneralOptions.Instance;

        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        if (IsInternalPath(fullPath) &&
            options.ShowInternalFilePaths == ShowInternalFilePathsOption.ShowFileName)
        {
            return GetPath(fullPath, options.DirectorySeparator,
                PathDisplayOption.TrailingPath, 1, options.SpaceAround);
        }

        return GetPath(fullPath, options.DirectorySeparator,
            options.PathDisplay, options.TrailingPathLevel, options.SpaceAround);
    }

    private static string GetPath(string fullPath, DirectorySeparatorOption separator,
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

        return path.Replace(System.IO.Path.DirectorySeparatorChar.ToString(), replacement);
    }

    private static string GetSeparatorString(DirectorySeparatorOption option)
    {
        return option switch
        {
            DirectorySeparatorOption.Backslash => "\\",
            DirectorySeparatorOption.Slash => "/",
            DirectorySeparatorOption.GreaterThan => ">",
            DirectorySeparatorOption.LessThan => "<",
            DirectorySeparatorOption.Hyphen => "-",
            DirectorySeparatorOption.Colon => ":",
            _ => System.IO.Path.DirectorySeparatorChar.ToString()
        };
    }

    private static string GetTrailingPath(string fullPath, int segments)
    {
        string[] parts = fullPath.Split(new[]
        {
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar
        }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length <= segments)
            return fullPath;

        return System.IO.Path.Combine(parts.Skip(parts.Length - segments).ToArray());
    }

    private static string TrimPathFromStart(string path, int maxLength)
    {
        if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
            return path;

        return "…" + path.Substring(path.Length - (maxLength - 1));
    }

    private int CalculateMaxCharacters(double maxWidth)
    {
        double charWidth = 8.0;
        return Math.Max(20, Math.Min((int)((maxWidth - 20) / charWidth), 100));
    }

    private void UpdateBottomControlSize()
    {
        _lblFilePath.Measure(new Size(400, double.PositiveInfinity));
        double width = Math.Min(Math.Max(_lblFilePath.DesiredSize.Width, 100), 400);
        Width = width;
        _lblFilePath.Width = width;
        Height = _lblFilePath.DesiredSize.Height;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
            HandleRightClick(e.ClickCount);
        else
            HandleLeftClick(e.ClickCount);
    }

    private void HandleRightClick(int clickCount)
    {
        // Context menu handles all right-click functionality
    }

    private void HandleLeftClick(int clickCount)
    {
        if (clickCount >= 3)
        {
            _lblFilePath.SelectAll();
        }
    }


    private void OpenContainingFolder()
    {
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{_documentMonitor.FileName}\"");
        }
        catch
        {
        }
    }

    private ContextMenu CreateContextMenu()
    {
        ContextMenu menu = new ContextMenu();

        MenuItem copyFileName = new MenuItem { Header = "Copy File Name" };
        copyFileName.Click += (s, e) => CopyToClipboard(GetFileName());

        MenuItem copyFullPath = new MenuItem { Header = "Copy Full Path" };
        copyFullPath.Click += (s, e) => CopyToClipboard(GetFullPath());

        MenuItem copyProjectRelative = new MenuItem { Header = "Copy Project Relative Path" };
        copyProjectRelative.Click += (s, e) => CopyToClipboard(GetProjectRelativePath());

        MenuItem copySolutionRelative = new MenuItem { Header = "Copy Solution Relative Path" };
        copySolutionRelative.Click += (s, e) => CopyToClipboard(GetSolutionRelativePath());

        menu.Items.Add(copyFileName);
        menu.Items.Add(copyFullPath);
        menu.Items.Add(copyProjectRelative);
        menu.Items.Add(copySolutionRelative);

        string? ext = System.IO.Path.GetExtension(_documentMonitor.FileName)?.ToLower();
        if (ext == ".cs" || ext == ".vb" || ext == ".fs")
        {
            MenuItem copyNamespace = new MenuItem { Header = "Copy Namespace" };
            copyNamespace.Click += (s, e) => CopyToClipboard(GetNamespace());
            menu.Items.Add(copyNamespace);
        }

        menu.Items.Add(new Separator());

        MenuItem openFolder = new MenuItem { Header = "Open Containing Folder" };
        openFolder.Click += (s, e) => OpenContainingFolder();
        menu.Items.Add(openFolder);

        return menu;
    }

    private void CopyToClipboard(string text)
    {
        try
        {
            if (!string.IsNullOrEmpty(text))
                Clipboard.SetText(text);
        }
        catch
        {
        }
    }

    private string GetFileName()
    {
        string fullPath = _documentMonitor.FileName;
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

        try
        {
            string? projectDir = FindProjectDirectory(fullPath);
            if (!string.IsNullOrEmpty(projectDir) && fullPath.StartsWith(projectDir, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(projectDir.Length).TrimStart('\\', '/');
            }
        }
        catch
        {
        }

        return fullPath;
    }

    private string GetSolutionRelativePath()
    {
        string? fullPath = _documentMonitor.FileName;
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        try
        {
            string? solutionDir = FindSolutionDirectory(fullPath);
            if (!string.IsNullOrEmpty(solutionDir) && fullPath.StartsWith(solutionDir, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(solutionDir.Length).TrimStart('\\', '/');
            }
        }
        catch
        {
        }

        return fullPath;
    }

    private string? FindProjectDirectory(string filePath)
    {
        string? dir = System.IO.Path.GetDirectoryName(filePath);
        while (!string.IsNullOrEmpty(dir))
        {
            if (System.IO.Directory.GetFiles(dir, "*.csproj").Length > 0 ||
                System.IO.Directory.GetFiles(dir, "*.vbproj").Length > 0 ||
                System.IO.Directory.GetFiles(dir, "*.fsproj").Length > 0)
            {
                return dir;
            }
            dir = System.IO.Path.GetDirectoryName(dir);
        }
        return null;
    }

    private string? FindSolutionDirectory(string filePath)
    {
        string? dir = System.IO.Path.GetDirectoryName(filePath);
        while (!string.IsNullOrEmpty(dir))
        {
            if (System.IO.Directory.GetFiles(dir, "*.sln").Length > 0 ||
                System.IO.Directory.GetFiles(dir, "*.slnx").Length > 0)
            {
                return dir;
            }
            dir = System.IO.Path.GetDirectoryName(dir);
        }
        return null;
    }

    private string GetNamespace()
    {
        try
        {
            string? ext = System.IO.Path.GetExtension(_documentMonitor.FileName)?.ToLower();
            if (ext != ".cs" && ext != ".vb" && ext != ".fs")
                return string.Empty;

            ITextSnapshot snapshot = _textView.TextSnapshot;
            string text = snapshot.GetText();

            Match match;

            if (ext == ".cs" || ext == ".fs")
            {
                match = CSharpNamespaceRegex.Match(text);
                if (match.Success)
                    return match.Groups[1].Value;
            }
            else if (ext == ".vb")
            {
                match = VbNamespaceRegex.Match(text);
                if (match.Success)
                    return match.Groups[1].Value;
            }
        }
        catch
        {
        }

        return string.Empty;
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
        if (_isDisposed)
            return;

        if (_documentMonitor != null)
            _documentMonitor.PropertyChanged -= OnDocumentFileNameChanged;

        GC.SuppressFinalize(this);
        _isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(FilePathMargin));
    }
}

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace FilePathOnDocument.Core;

internal class DocumentMonitor : INotifyPropertyChanged, IDisposable
{
    private readonly ITextDocument? _document;
    private string? _fileName;
    private bool _isDisposed;

    public DocumentMonitor(ITextDocument? document)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (document == null)
            return;

        _document = document;
        SetFileName(_document.FilePath);
        _document.FileActionOccurred += OnFileActionOccurred;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? FileName => _fileName;
    public string? Namespace { get; private set; }
    public string? ProjectDir { get; private set; }
    public string? ProjectName { get; private set; }
    public string? SolutionDir { get; private set; }
    public string? SolutionName { get; private set; }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public bool IsInSolutionContext()
    {
        return ProjectDir != null && SolutionDir != null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _document?.FileActionOccurred -= OnFileActionOccurred;
        }

        _isDisposed = true;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static readonly Regex CSharpNamespaceRegex = new(@"namespace\s+([\w\.]+)", RegexOptions.Compiled);
    private static readonly Regex VbNamespaceRegex = new(@"Namespace\s+([\w\.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, Regex> NamespaceExtractors = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".cs", CSharpNamespaceRegex },
        { ".fs", CSharpNamespaceRegex },
        { ".vb", VbNamespaceRegex }
    };

    private void ExtractNamespace(string? filePath)
    {
        Namespace = null;

        if (string.IsNullOrEmpty(filePath) || _document == null)
            return;

        string? ext = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(ext) || !NamespaceExtractors.TryGetValue(ext, out Regex? regex))
            return;

        try
        {
            string text = _document.TextBuffer.CurrentSnapshot.GetText();
            Match match = regex.Match(text);
            if (match.Success)
                Namespace = match.Groups[1].Value;
        }
        catch
        {
            // Silent failure
        }
    }

    private void OnFileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        SetFileName(e.FilePath);
    }

    private void ResolveProjectAndSolution(string? filePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        ProjectDir = null;
        ProjectName = null;
        SolutionDir = null;
        SolutionName = null;

        if (string.IsNullOrEmpty(filePath))
            return;

        if (Package.GetGlobalService(typeof(SVsRunningDocumentTable)) is IVsRunningDocumentTable rdt)
        {
            rdt.FindAndLockDocument(0, filePath, out IVsHierarchy? hier, out _, out _, out uint cookie);
            if (cookie != 0)
                rdt.UnlockDocument(0, cookie);

            if (hier != null)
            {
                hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out object projDirObj);
                hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectName, out object projNameObj);
                ProjectDir = projDirObj as string;
                ProjectName = projNameObj as string;
            }
        }

        if (Package.GetGlobalService(typeof(SVsSolution)) is IVsSolution solution)
        {
            solution.GetSolutionInfo(out string? slnDir, out string? slnName, out _);
            SolutionDir = slnDir;
            SolutionName = slnName;
        }
    }

    private void SetFileName(string? filePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_fileName == filePath)
            return;

        _fileName = filePath;
        ResolveProjectAndSolution(filePath);
        ExtractNamespace(filePath);
        OnPropertyChanged(nameof(FileName));
    }
}

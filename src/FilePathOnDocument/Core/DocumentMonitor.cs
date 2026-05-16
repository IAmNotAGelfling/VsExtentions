using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Text;

namespace FilePathOnDocument.Core;

internal class DocumentMonitor : INotifyPropertyChanged, IDisposable
{
    private string? _fileName;
    private bool _isDisposed;
    private readonly ITextDocument? _document;

    public event PropertyChangedEventHandler? PropertyChanged;

    public DocumentMonitor(ITextDocument? document)
    {
        if (document == null)
            return;

        _document = document;
        FileName = _document.FilePath;
        _document.FileActionOccurred += OnFileActionOccurred;
    }

    private void OnFileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
    {
        FileName = e.FilePath;
    }

    public string? FileName
    {
        get => _fileName;
        private set
        {
            if (_fileName == value)
                return;

            _fileName = value;
            OnPropertyChanged(nameof(FileName));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        if (_document != null)
            _document.FileActionOccurred -= OnFileActionOccurred;

        _isDisposed = true;
    }
}

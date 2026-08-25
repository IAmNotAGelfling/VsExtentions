using System;
using FilePathOnDocument.Core;
using FilePathOnDocument.Options;
using Microsoft.VisualStudio.Text.Editor;

namespace FilePathOnDocument.Margin;

internal abstract class FilePathMarginProvider : IWpfTextViewMarginProvider
{
    protected abstract AlignmentOption Alignment { get; }

    public IWpfTextViewMargin? CreateMargin(IWpfTextViewHost wpfTextViewHost,
        IWpfTextViewMargin marginContainer)
    {
        if (wpfTextViewHost?.TextView == null)
            return null;

        GeneralOptions options = GeneralOptions.Instance;

        if (options.Alignment != Alignment)
            return null;

        try
        {
            FilePathMargin margin = new(wpfTextViewHost.TextView, Alignment);

            GeneralOptions.Saved += _ => margin.RefreshFromSettings();
            InternalPathsOptions.Saved += _ => margin.RefreshFromSettings();

            return margin;
        }
        catch
        {
            return null;
        }
    }
}

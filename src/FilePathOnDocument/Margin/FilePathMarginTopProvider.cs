using System.ComponentModel.Composition;
using FilePathOnDocument.Core;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace FilePathOnDocument.Margin;

[Export(typeof(IWpfTextViewMarginProvider))]
[Name(FilePathMargin.MarginName + "Top")]
[Order(After = PredefinedMarginNames.Top)]
[MarginContainer(PredefinedMarginNames.Top)]
[ContentType("text")]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class FilePathMarginTopProvider : FilePathMarginProvider
{
    protected override AlignmentOption Alignment => AlignmentOption.Top;
}

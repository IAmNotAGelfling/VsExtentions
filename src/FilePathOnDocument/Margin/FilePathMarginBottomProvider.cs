using System.ComponentModel.Composition;
using FilePathOnDocument.Core;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace FilePathOnDocument.Margin;

[Export(typeof(IWpfTextViewMarginProvider))]
[Name(FilePathMargin.MarginName + "Bottom")]
[Order(After = "Wpf Horizontal Scrollbar")]
[MarginContainer(PredefinedMarginNames.Bottom)]
[ContentType("text")]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class FilePathMarginBottomProvider : FilePathMarginProvider
{
    protected override AlignmentOption Alignment => AlignmentOption.Bottom;
    protected override string MarginName => FilePathMargin.MarginName + "Bottom";
}

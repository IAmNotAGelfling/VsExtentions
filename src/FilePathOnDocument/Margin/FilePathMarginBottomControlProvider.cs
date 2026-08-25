using System.ComponentModel.Composition;
using FilePathOnDocument.Core;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace FilePathOnDocument.Margin;

[Export(typeof(IWpfTextViewMarginProvider))]
[Name(FilePathMargin.MarginName + "BottomControl")]
[Order(Before = "Wpf Horizontal Scrollbar")]
[MarginContainer(PredefinedMarginNames.BottomRightCorner)]
[ContentType("text")]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class FilePathMarginBottomControlProvider : FilePathMarginProvider
{
    protected override AlignmentOption Alignment => AlignmentOption.BottomControl;
}

using System.ComponentModel;
using Community.VisualStudio.Toolkit;
using FilePathOnDocument.Core;
using FilePathOnDocument.Converters;

namespace FilePathOnDocument.Options;

internal class GeneralOptions : BaseOptionModel<GeneralOptions>
{
    [Category("Path Display")]
    [DisplayName("Display Path Format")]
    [Description("Select the format for displaying the file path in the margin.")]
    [DefaultValue(PathDisplayOption.Absolute)]
    [TypeConverter(typeof(EnumConverter))]
    public PathDisplayOption PathDisplay { get; set; } = PathDisplayOption.Absolute;

    [Category("Path Display")]
    [DisplayName("Trailing Path Level")]
    [Description("Specify the number of trailing folders to show when using the trailing path format (1-10).")]
    [DefaultValue(2)]
    [TypeConverter(typeof(IntRangeConverter))]
    public int TrailingPathLevel { get; set; } = 2;

    [Category("Path Display")]
    [DisplayName("Directory Separator")]
    [Description("Character used to separate folders in the path.")]
    [DefaultValue(DirectorySeparatorOption.Default)]
    [TypeConverter(typeof(DisplayNameEnumConverter))]
    public DirectorySeparatorOption DirectorySeparator { get; set; } = DirectorySeparatorOption.Default;

    [Category("Path Display")]
    [DisplayName("Space Around")]
    [Description("Add space around the directory separator.")]
    [DefaultValue(false)]
    public bool SpaceAround { get; set; } = false;

    [Category("Visibility Options")]
    [DisplayName("Disable Extension")]
    [Description("Disable the file path footer extension.")]
    [DefaultValue(false)]
    public bool ExtensionDisabled { get; set; } = false;

    [Category("Visibility Options")]
    [DisplayName("Path Alignment")]
    [Description("Position of the file path in the margin (requires document restart).")]
    [DefaultValue(AlignmentOption.Bottom)]
    [TypeConverter(typeof(EnumConverter))]
    public AlignmentOption Alignment { get; set; } = AlignmentOption.Bottom;

    [Category("Visibility Options")]
    [DisplayName("Internal File Paths")]
    [Description("Control how internal file paths are displayed. Configure the Internal Paths from the Options.")]
    [DefaultValue(ShowInternalFilePathsOption.Hide)]
    [TypeConverter(typeof(EnumConverter))]
    public ShowInternalFilePathsOption ShowInternalFilePaths { get; set; } = ShowInternalFilePathsOption.Hide;
}

using System.ComponentModel.DataAnnotations;

namespace FilePathOnDocument.Core;

public enum AlignmentOption
{
    Top,
    Bottom,
    [Display(Name = "Bottom (Inline)")]
    BottomControl,
}

public enum PathDisplayOption
{
    Absolute,
    TrailingPath,
}

public enum OpenFolderOption
{
    CtrlClick,
    DoubleRightClick,
}

public enum CopyPathOption
{
    RightClick,
    LeftClick,
}

public enum CopyPathFormatOption
{
    Absolute,
    DisplayedPath,
}

public enum DirectorySeparatorOption
{
    [Display(Name = "Default")]
    Default,
    [Display(Name = "\\")]
    Backslash,
    [Display(Name = "/")]
    Slash,
    [Display(Name = ">")]
    GreaterThan,
    [Display(Name = "<")]
    LessThan,
    [Display(Name = "-")]
    Hyphen,
    [Display(Name = ":")]
    Colon,
}

public enum ShowInternalFilePathsOption
{
    Show,
    Hide,
    ShowFileName,
}

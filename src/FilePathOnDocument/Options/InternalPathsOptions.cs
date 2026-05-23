using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FilePathOnDocument.Options;

internal class InternalPathsOptions : BaseOptionModel<InternalPathsOptions>
{
    [Category("Internal Paths")]
    [DisplayName("Internal Path List")]
    [Description("Newline-separated list of paths to treat as internal (e.g., 'Temp', 'AppData\\Local\\Temp').")]
    [DefaultValue("Temp\nAppData\\Local\\Temp")]
    public string InternalPaths { get; set; } = "Temp\nAppData\\Local\\Temp";

    public IEnumerable<string> GetPaths()
    {
        if (string.IsNullOrWhiteSpace(InternalPaths))
            return [];

        return InternalPaths
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p));
    }
}

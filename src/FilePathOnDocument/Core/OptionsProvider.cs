using Community.VisualStudio.Toolkit;
using System.Runtime.InteropServices;

namespace FilePathOnDocument.Core;

internal partial class OptionsProvider
{
    [ComVisible(true)]
    public class GeneralOptionsPage : BaseOptionPage<Options.GeneralOptions>;

    [ComVisible(true)]
    public class InternalPathsOptionsPage : BaseOptionPage<Options.InternalPathsOptions>;
}

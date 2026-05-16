using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using FilePathOnDocument.Core;
using Microsoft.VisualStudio.Shell;

namespace FilePathOnDocument;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration("File Path On Document", "Displays file paths in the editor margin with configurable options.", "2.0")]
[ProvideOptionPage(typeof(OptionsProvider.GeneralOptionsPage), "File Path On Document", "General", 0, 0, true)]
[ProvideProfile(typeof(OptionsProvider.GeneralOptionsPage), "File Path On Document", "General", 0, 0, true)]
[ProvideOptionPage(typeof(OptionsProvider.InternalPathsOptionsPage), "File Path On Document", "Internal Files", 0, 0, true)]
[ProvideProfile(typeof(OptionsProvider.InternalPathsOptionsPage), "File Path On Document", "Internal Files", 0, 0, true)]
[Guid(FilePathOnDocumentPackage.PackageGuidString)]
public sealed class FilePathOnDocumentPackage : ToolkitPackage
{
    public const string PackageGuidString = "02e4fc97-5aa3-47ff-8f24-a7230dbaa67f";

    protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
    }
}

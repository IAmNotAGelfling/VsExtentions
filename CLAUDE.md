# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This repository contains Visual Studio extensions built using the **Visual Studio Community Toolkit**. Currently includes:

- **FilePathOnDocument**: Displays file paths in the VS editor margin with configurable display options and quick copy actions

## Build Commands

```bash
# Build the solution (Debug configuration by default)
dotnet build FilePathOnDocument.slnx

# Build in Release configuration
dotnet build FilePathOnDocument.slnx -c Release

# Clean build artifacts
dotnet clean FilePathOnDocument.slnx
```

## Project Structure

### Solution Layout
```
VsExtensions/
├── src/
│   └── FilePathOnDocument/          # VSIX extension project
│       ├── Margin/                   # Text view margin providers and UI
│       ├── Options/                  # Settings/options pages
│       ├── Core/                     # Core logic (monitor, enums)
│       └── Converters/               # Type converters for options UI
├── docs/
│   └── superpowers/                  # Design specs and plans
└── FilePathOnDocument.slnx          # Solution file (XML-based)
```

### Extension Architecture

**Technology Stack:**
- **Target Framework**: .NET Framework 4.8
- **Language**: C# 14 (with file-scoped namespaces, nullable reference types)
- **VS Extension SDK**: Visual Studio Community Toolkit 17.x
- **Base Classes**: `ToolkitPackage`, `BaseOptionModel<T>`, `IWpfTextViewMargin`

**Core Components:**

1. **FilePathOnDocumentPackage** (`FilePathOnDocumentPackage.cs`)
   - Main VSIX package entry point
   - Inherits from `ToolkitPackage`
   - Registers options pages via attributes

2. **Options System** (`Options/`)
   - `GeneralOptions`: Main settings (path display, alignment, separators)
   - `InternalPathsOptions`: Configurable list of internal paths to filter
   - Both inherit from `BaseOptionModel<T>` (Community Toolkit pattern)
   - Settings persist automatically via toolkit

3. **Margin Providers** (`Margin/`)
   - `FilePathMarginProvider`: Abstract base provider
   - Three concrete MEF exports for different positions:
     - `FilePathMarginTopProvider`: Top margin (above editor)
     - `FilePathMarginBottomProvider`: Bottom margin (below scrollbar)
     - `FilePathMarginBottomControlProvider`: Bottom-right inline control
   - Only the provider matching the user's alignment setting creates a margin

4. **FilePathMargin** (`Margin/FilePathMargin.cs`)
   - The actual WPF UI component (Canvas containing TextBox)
   - Implements `IWpfTextViewMargin`
   - Handles display formatting, click actions, path filtering
   - Subscribes to settings changes for live updates
   - Context menu for copy actions (file name, full path, project-relative, solution-relative, namespace)

5. **DocumentMonitor** (`Core/DocumentMonitor.cs`)
   - Tracks file path changes in the text view
   - Implements `INotifyPropertyChanged`
   - Raises events when file is saved/renamed
   - Margin subscribes to these events to update display

6. **Enums** (`Core/Enums.cs`)
   - `AlignmentOption`: Top, Bottom, BottomControl
   - `PathDisplayOption`: Absolute, TrailingPath
   - `DirectorySeparatorOption`: Default, Backslash, Slash, GreaterThan, LessThan, Hyphen, Colon
   - `ShowInternalFilePathsOption`: Show, Hide, ShowFileName
   - Uses `[Display(Name = "...")]` attributes for friendly names in UI

### Data Flow

**Margin Creation:**
1. VS opens a text view
2. MEF invokes all `IWpfTextViewMarginProvider` exports
3. Each provider checks if `GeneralOptions.Instance.Alignment` matches its position
4. Matching provider creates `FilePathMargin` instance
5. Margin subscribes to `GeneralOptions.Saved` and `InternalPathsOptions.Saved` events
6. Margin creates `DocumentMonitor` to track file path changes

**Display Updates:**
1. `DocumentMonitor.FileName` property changes (file saved/renamed)
2. Raises `PropertyChanged` event
3. Margin's `OnDocumentFileNameChanged` handler calls `UpdateDisplay()`
4. `UpdateDisplay()` reads current settings, formats path, updates TextBox

**Settings Changes:**
1. User modifies settings in Tools → Options → File Path On Document
2. Community Toolkit persists to registry automatically
3. `GeneralOptions.Saved` event fires
4. All active margins receive event, call `RefreshFromSettings()` → `UpdateDisplay()`

## Development Guidelines

### Adding New Options

Options are managed via `BaseOptionModel<T>` from Community Toolkit:

```csharp
[Category("Category Name")]
[DisplayName("Option Label")]
[Description("Help text shown in options UI")]
[DefaultValue(DefaultValue)]
[TypeConverter(typeof(ConverterType))]  // If needed
public OptionType OptionName { get; set; } = DefaultValue;
```

Access options in code: `GeneralOptions.Instance.OptionName`

### MEF Exports

Margin providers use MEF attributes for VS integration:

```csharp
[Export(typeof(IWpfTextViewMarginProvider))]
[Name("MarginName")]
[Order(After = PredefinedMarginNames.Top)]  // or Before
[MarginContainer(PredefinedMarginNames.Top)]  // Top, Bottom, BottomRightCorner
[ContentType("text")]
[TextViewRole(PredefinedTextViewRoles.Document)]
```

### VS Theme Integration

Use `EnvironmentColors` for theme-aware styling:

```csharp
SetResourceReference(BackgroundProperty, EnvironmentColors.ScrollBarBackgroundBrushKey);
_lblFilePath.SetResourceReference(ForegroundProperty, EnvironmentColors.ComboBoxTextBrushKey);
```

### Disposal Pattern

Components must implement `IDisposable` correctly:
- Unsubscribe from all events in `Dispose()`
- Set `_isDisposed` flag and check in public methods
- Call `GC.SuppressFinalize(this)` if no finalizer

### Error Handling

Silent failures in UI code to prevent crashing VS:

```csharp
try
{
    // Clipboard, Process.Start, file operations
}
catch
{
    // Silent - don't crash VS
}
```

## Common Tasks

### Building VSIX for Distribution

```bash
# Build in Release mode
dotnet build FilePathOnDocument.slnx -c Release

# VSIX output location
# src/FilePathOnDocument/bin/Release/net48/FilePathOnDocument.vsix
```

### Testing Changes

1. Build the solution (F5 in VS launches experimental instance)
2. Test in VS Experimental Instance
3. Verify settings persist via Tools → Options → File Path On Document
4. Test margin positioning with different alignment options (requires reopening documents)

### Debugging

- Package initialization: Set breakpoint in `FilePathOnDocumentPackage.InitializeAsync()`
- Margin creation: Set breakpoint in `FilePathMarginProvider.CreateMargin()`
- Display updates: Set breakpoint in `FilePathMargin.UpdateDisplay()`
- Settings changes: Set breakpoint in `RefreshFromSettings()`

## VSIX Manifest

Located at `src/FilePathOnDocument/source.extension.vsixmanifest`:
- Version number (update for releases)
- Display name and description
- VS version requirements (currently VS 2022 17.14+)
- Icon reference

## Known Constraints

- **Alignment Changes**: Require reopening documents to take effect (by design)
- **Bottom Control Width**: Fixed range 100-400px, cannot dynamically resize beyond this
- **Internal Path Detection**: String-based matching, may have edge cases with symlinks
- **Target Framework**: Must remain .NET Framework 4.8 (required by VS 2022 in-process extensions)

## Repository Conventions

- Use file-scoped namespaces
- Explicit types over `var` with abbreviated `new` expressions: `List<string> items = new();`
- Collection expressions and initializers preferred
- Nullable reference types enabled
- C# 14 language features allowed

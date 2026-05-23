# Visual Studio Extensions Collection

A collection of Visual Studio extensions built with the **Visual Studio Community Toolkit**, designed to enhance the development experience in Visual Studio 2022.

## Extensions

### FilePathOnDocument

Displays file paths in the Visual Studio editor margin with extensive customization options.

**Features:**
- **Multiple Position Options**: Display at top, bottom, or bottom-right corner
- **Flexible Path Formats**: Show absolute paths or trailing path segments
- **Custom Separators**: Choose from backslash, forward slash, or symbols (>, <, -, :)
- **Quick Actions**: Right-click context menu with multiple copy options:
  - Copy file name only
  - Copy full path
  - Copy project-relative path
  - Copy solution-relative path
  - Copy namespace (for C#/VB/F# files)
- **Internal Path Filtering**: Hide or show file paths for temporary/internal files
- **Theme Integration**: Automatically adapts to Visual Studio themes

**Installation:**
1. Download the `.vsix` file from the [Releases](releases) page
2. Double-click to install in Visual Studio 2022
3. Configure via **Tools → Options → File Path On Document**

## Building from Source

### Prerequisites
- Visual Studio 2022 (version 17.14 or later)
- .NET SDK 10.0 or later
- Visual Studio Extension Development workload

### Build Instructions

```bash
# Clone the repository
git clone https://github.com/IAmNotAGelfling/VsExtentions.git
cd VsExtentions

# Build the solution
dotnet build FilePathOnDocument.slnx

# Build in Release configuration
dotnet build FilePathOnDocument.slnx -c Release
```

The compiled VSIX packages will be in:
```
src/<ExtensionName>/bin/Release/net48/<ExtensionName>.vsix
```

## Technology Stack

- **Target Framework**: .NET Framework 4.8
- **Language**: C# 14
- **SDK**: Visual Studio Community Toolkit 17.x
- **Architecture**: MEF (Managed Extensibility Framework)

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

### Development Guidelines

- Follow the existing code style and patterns
- Use file-scoped namespaces
- Enable nullable reference types
- Prefer explicit types over `var`
- Use collection expressions where appropriate
- Ensure all `IDisposable` implementations properly unsubscribe from events

See [CLAUDE.md](CLAUDE.md) for detailed architecture and development guidance.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**IAmNotAGelfling**
- GitHub: [@IAmNotAGelfling](https://github.com/IAmNotAGelfling)

## Acknowledgements

- Built with [Visual Studio Community Toolkit](https://github.com/VsixCommunity/Community.VisualStudio.Toolkit)
- Icons and resources from Visual Studio SDK

## Support

If you encounter issues or have feature requests:
1. Check existing [Issues](https://github.com/IAmNotAGelfling/VsExtentions/issues)
2. Create a new issue with detailed description
3. Include Visual Studio version and extension version

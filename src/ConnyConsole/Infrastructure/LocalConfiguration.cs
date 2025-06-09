using System.IO.Abstractions;

namespace ConnyConsole.Infrastructure;

/// <summary>
/// Provides functionality for handling local-level (current working directory) configuration files.
/// </summary>
public sealed class LocalConfiguration : IConfigurationPathProvider
{
    private const string ConfigDirectoryName = $".{App.Name}";
    private const string LocalConfigFileName = "config";

    private readonly IFileSystem _fileSystem;

    private string? _localConfigFilePath;

    /// <summary>
    /// Provides functionality for managing the local (working directory) configuration file path within the application.
    /// This allows testable and abstracted access to file system and environment operations.
    /// </summary>
    /// <param name="fileSystem">
    /// Abstraction instance of the file system. This allows for testability and abstraction of file system operations.
    /// </param>
    public LocalConfiguration(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Returns the local (working directory) configuration file's full path.
    /// </summary>
    /// <returns>The full file path.</returns>
    public string GetConfigFilePath()
    {
        if (!string.IsNullOrWhiteSpace(_localConfigFilePath))
        {
            return _localConfigFilePath;
        }

        var systemConfigDirectory = GetLocalConfigDirectoryPath();
        _localConfigFilePath = _fileSystem.Path.Combine(systemConfigDirectory, LocalConfigFileName);

        return _localConfigFilePath;
    }

    /// <summary>
    /// Retrieves the path to the current working directory config folder, combining the current folder path with the application name.
    /// </summary>
    /// <returns>
    /// The full path to the system configuration directory as a string.
    /// </returns>
    private string GetLocalConfigDirectoryPath()
    {
        var currentDirectoryPath = _fileSystem.Directory.GetCurrentDirectory();

        return _fileSystem.Path.Combine(currentDirectoryPath, ConfigDirectoryName.ToLowerInvariant());
    }
}

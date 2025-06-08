using System.IO.Abstractions;

namespace ConnyConsole.Infrastructure;

/// <summary>
/// Provides functionality for handling local-level (current working directory) configuration files.
/// </summary>
public static class LocalConfiguration
{
    private const string ConfigDirectoryName = $".{App.Name}";
    private const string LocalConfigFileName = "config";
    private static string? _localConfigFilePath;

    /// <summary>
    /// Returns the local (working directory) configuration file's full path.
    /// </summary>
    /// <param name="fileSystem">
    /// Abstraction instance of the file system. This allows for testability and abstraction of file system operations.
    /// </param>
    /// <returns>The full file path.</returns>
    internal static string GetConfigFilePath(IFileSystem fileSystem)
    {
        if (!string.IsNullOrWhiteSpace(_localConfigFilePath))
        {
            return _localConfigFilePath;
        }

        var systemConfigDirectory = GetLocalConfigDirectoryPath(fileSystem);
        _localConfigFilePath = fileSystem.Path.Combine(systemConfigDirectory, LocalConfigFileName);

        return _localConfigFilePath;
    }

    /// <summary>
    /// Retrieves the path to the current working directory config folder, combining the current folder path with the application name.
    /// </summary>
    /// <param name="fileSystem">
    /// Abstraction instance of the file system. This allows for testability and abstraction of file system operations.
    /// </param>
    /// <returns>
    /// The full path to the system configuration directory as a string.
    /// </returns>
    private static string GetLocalConfigDirectoryPath(IFileSystem fileSystem)
    {
        var currentDirectoryPath = fileSystem.Directory.GetCurrentDirectory();

        return fileSystem.Path.Combine(currentDirectoryPath, ConfigDirectoryName.ToLowerInvariant());
    }
}

using System.IO.Abstractions;

namespace ConnyConsole.Infrastructure;

/// <summary>
/// Provides functionality for handling system-level configuration files.
/// </summary>
public static class SystemConfiguration
{
    private const string SystemConfigFileName = "config";
    private static string? _systemConfigFilePath;

    /// <summary>
    /// Returns the system configuration file's full path.
    /// It's in an app-specific subfolder located in the <see cref="Environment.SpecialFolder.CommonApplicationData"/> folder.
    /// <para>On Windows, for instance: "C:\ProgramData\ConnyConsole\config"</para>
    /// </summary>
    /// <param name="fileSystem">
    /// Abstraction instance of the file system. This allows for testability and abstraction of file system operations.
    /// </param>
    /// <returns>The full file path.</returns>
    internal static string GetConfigFilePath(IFileSystem fileSystem)
    {
        if (!string.IsNullOrWhiteSpace(_systemConfigFilePath))
        {
            return _systemConfigFilePath;
        }

        var systemConfigDirectory = GetSystemConfigDirectoryPath(fileSystem);
        _systemConfigFilePath = fileSystem.Path.Combine(systemConfigDirectory, SystemConfigFileName);

        return _systemConfigFilePath;
    }

    /// <summary>
    /// Retrieves the path to the system configuration directory, combining the common application data path with the application name.
    /// </summary>
    /// <param name="fileSystem">
    /// Abstraction instance of the file system. This allows for testability and abstraction of file system operations.
    /// </param>
    /// <returns>
    /// The full path to the system configuration directory as a string.
    /// </returns>
    private static string GetSystemConfigDirectoryPath(IFileSystem fileSystem)
    {
        var commonApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        return fileSystem.Path.Combine(commonApplicationDataPath, App.Name);
    }
}

using System.IO.Abstractions;

namespace ConnyConsole.Infrastructure;

/// <summary>
/// Provides functionality for handling system-level configuration files.
/// </summary>
public sealed class SystemConfiguration : IConfigurationPathProvider
{
    private const string SystemConfigFileName = "config";

    private readonly IFileSystem _fileSystem;
    private readonly IEnvironmentProvider _environmentProvider;

    private string? _systemConfigFilePath;

    /// <summary>
    /// Provides functionality for managing the system-level configuration file path within the application.
    /// This allows testable and abstracted access to file system and environment operations.
    /// </summary>
    /// <param name="fileSystem">
    /// Abstraction instance of the file system. This allows for testability and abstraction of file system operations.
    /// </param>
    /// <param name="environmentProvider">
    /// Abstraction instance of the environment. This allows for testability and abstraction of environment-specific functions.
    /// </param>
    public SystemConfiguration(IFileSystem fileSystem, IEnvironmentProvider environmentProvider)
    {
        _fileSystem = fileSystem;
        _environmentProvider = environmentProvider;
    }

    /// <summary>
    /// Returns the system configuration file's full path.
    /// It's in an app-specific subfolder located in the <see cref="Environment.SpecialFolder.CommonApplicationData"/> folder.
    /// <para>On Windows, for instance: "C:\ProgramData\ConnyConsole\config"</para>
    /// </summary>
    /// <returns>The full file path.</returns>
    public string GetConfigFilePath()
    {
        if (!string.IsNullOrWhiteSpace(_systemConfigFilePath))
        {
            return _systemConfigFilePath;
        }

        var systemConfigDirectory = GetSystemConfigDirectoryPath();
        _systemConfigFilePath = _fileSystem.Path.Combine(systemConfigDirectory, SystemConfigFileName);

        return _systemConfigFilePath;
    }

    /// <summary>
    /// Retrieves the path to the system configuration directory, combining the common application data path with the application name.
    /// </summary>
    /// <returns>
    /// The full path to the system configuration directory as a string.
    /// </returns>
    private string GetSystemConfigDirectoryPath()
    {
        var commonApplicationDataPath = _environmentProvider.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        return _fileSystem.Path.Combine(commonApplicationDataPath, App.Name);
    }
}

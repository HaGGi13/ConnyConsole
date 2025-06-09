using System.IO.Abstractions;

namespace ConnyConsole.Infrastructure;

/// <summary>
/// Provides functionality for handling user-level (global) configuration files.
/// </summary>
public sealed class GlobalConfiguration : IConfigurationPathProvider
{
    private const string GlobalConfigFileName = ".connyconfig";

    private readonly IFileSystem _fileSystem;
    private readonly IEnvironmentProvider _environmentProvider;

    private string? _globalConfigFilePath;

    /// <summary>
    /// Provides functionality for managing the global (user-level) configuration file path within the application.
    /// This allows testable and abstracted access to file system and environment operations.
    /// </summary>
    /// <param name="fileSystem">
    /// Abstraction instance of the file system. This allows for testability and abstraction of file system operations.
    /// </param>
    /// <param name="environmentProvider">
    /// Abstraction instance of the environment. This allows for testability and abstraction of environment-specific functions.
    /// </param>
    public GlobalConfiguration(IFileSystem fileSystem, IEnvironmentProvider environmentProvider)
    {
        _fileSystem = fileSystem;
        _environmentProvider = environmentProvider;
    }

    /// <summary>
    /// Returns the global (user-level) configuration file's full path.
    /// It's in the <see cref="Environment.SpecialFolder.UserProfile"/> folder.
    /// <para>On Windows, for instance: "C:\Users\&lt;userprofile&gt;\.connyconfig"</para>
    /// </summary>
    /// <returns>The full file path.</returns>
    public string GetConfigFilePath()
    {
        if (!string.IsNullOrWhiteSpace(_globalConfigFilePath))
        {
            return _globalConfigFilePath;
        }

        var systemConfigDirectory = _environmentProvider.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _globalConfigFilePath = _fileSystem.Path.Combine(systemConfigDirectory, GlobalConfigFileName);

        return _globalConfigFilePath;
    }
}

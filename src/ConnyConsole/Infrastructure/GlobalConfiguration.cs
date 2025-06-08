using System.IO.Abstractions;

namespace ConnyConsole.Infrastructure;

/// <summary>
/// Provides functionality for handling user-level (global) configuration files.
/// </summary>
public static class GlobalConfiguration
{
    private const string GlobalConfigFileName = ".connyconfig";
    private static string? _globalConfigFilePath;

    /// <summary>
    /// Returns the global (user-level) configuration file's full path.
    /// It's in the <see cref="Environment.SpecialFolder.UserProfile"/> folder.
    /// <para>On Windows, for instance: "C:\Users\&lt;userprofile&gt;\.connyconfig"</para>
    /// </summary>
    /// <param name="fileSystem">
    /// Abstraction instance of the file system. This allows for testability and abstraction of file system operations.
    /// </param>
    /// <returns>The full file path.</returns>
    internal static string GetConfigFilePath(IFileSystem fileSystem)
    {
        if (!string.IsNullOrWhiteSpace(_globalConfigFilePath))
        {
            return _globalConfigFilePath;
        }

        var systemConfigDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _globalConfigFilePath = fileSystem.Path.Combine(systemConfigDirectory, GlobalConfigFileName);

        return _globalConfigFilePath;
    }
}

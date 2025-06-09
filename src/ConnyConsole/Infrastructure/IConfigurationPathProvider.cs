namespace ConnyConsole.Infrastructure;

/// <summary>
/// Defines a mechanism for providing configuration file paths.
/// </summary>
public interface IConfigurationPathProvider
{
    /// <summary>
    /// Provides the file path to a configuration file.
    /// </summary>
    /// <returns>The full path to the configuration file.</returns>
    string GetConfigFilePath();
}

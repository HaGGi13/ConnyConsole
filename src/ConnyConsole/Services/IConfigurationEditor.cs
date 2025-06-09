using ConnyConsole.Settings;

namespace ConnyConsole.Services;

/// <summary>
/// Provides functions to manage configuration.
/// </summary>
public interface IConfigurationEditor
{
    /// <summary>
    /// Sets a configuration's newValue identified by its key.
    /// </summary>
    /// <param name="settingKey">The setting key to set the newValue for.</param>
    /// <param name="newValue">The new configuration value to set.</param>
    /// <param name="scope">The configuration scope to set the value.</param>
    string SetValue(string settingKey, string newValue, ConfigurationScope scope);
}

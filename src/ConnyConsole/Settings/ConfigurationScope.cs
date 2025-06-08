namespace ConnyConsole.Settings;

/// <summary>
/// Represents the scope of a configuration setting.
/// </summary>
public enum ConfigurationScope
{
    /// <summary>
    /// The configuration scope is unspecified.
    /// Indicates that no specific scope has been determined or provided.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Local configuration specific to the current directory.
    /// </summary>
    Local,

    /// <summary>
    /// User-level configuration.
    /// </summary>
    Global,

    /// <summary>
    /// System-level configuration.
    /// </summary>
    System
}

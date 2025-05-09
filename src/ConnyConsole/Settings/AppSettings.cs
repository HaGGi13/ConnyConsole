using System.IO.Abstractions;
using System.Text.Json.Serialization;

namespace ConnyConsole.Settings;

public class AppSettings
{
    private const string SystemConfigFileName = "config.json";
    private static string? _systemConfigFilePath;

    public const string SectionName = "AppSettings";

    private static readonly Dictionary<string, object> SupportedSettingKeys = new()
    {
        ["LoopOutputInterval"] = true,
        ["Cancellation"] = new Dictionary<string, object> { ["Timeout"] = true }
    };

    public TimeSpan LoopOutputInterval { get; set; } = TimeSpan.FromSeconds(1);

    [JsonPropertyName("Cancellation")]
    public CancellationSettings Cancellation { get; set; } = new();

    /// <summary>
    /// Retrieves the path to the system configuration directory, combining the common application data path with the application name.
    /// </summary>
    /// <param name="fileSystem">
    /// The file system object used to combine paths. This allows for testability and abstraction of file system operations.
    /// </param>
    /// <returns>
    /// The full path to the system configuration directory as a string.
    /// </returns>
    /// <remarks>
    /// This method constructs the directory path by combining the common application data folder (obtained via <see cref="Environment.GetFolderPath"/>)
    /// with the application's name (stored in <see cref="App.Name"/>). The resulting path points to a directory where system configuration files
    /// are typically stored.
    /// </remarks>
    private static string GetSystemConfigDirectoryPath(IFileSystem fileSystem)
    {
        var commonApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        return fileSystem.Path.Combine(commonApplicationDataPath, App.Name);
    }

    /// <summary>
    /// Returns the system configuration file's full path.
    /// It's in an app specific sub-folder located in the <see cref="Environment.SpecialFolder.CommonApplicationData"/> folder.
    /// <para>On Windows for instance: "C:\ProgramData\ConnyConsole\config.json"</para>
    /// </summary>
    /// <param name="fileSystem">Abstraction instance of the file system.</param>
    /// <returns>The full file path.</returns>
    internal static string GetSystemConfigFilePath(IFileSystem fileSystem)
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
    /// Determines whether the specified setting <paramref name="key"/> is valid and supported.
    /// </summary>
    /// <param name="key">The setting key to validate.</param>
    /// <returns><c>true</c> if the setting key is valid and supported; otherwise <c>false</c></returns>
    internal static bool IsValidSettingKey(string key)
    {
        return IsValidConfigKey(key);
    }

    /// <summary>
    /// Determines whether the specified setting <paramref name="key"/> is valid based on the structure of <c>SupportedSettingKeys</c> internal dictionary.
    /// </summary>
    /// <param name="key">The setting key to validate.</param>
    /// <returns><c>true</c> if the setting key exists and is valid according to <c>SupportedSettingKeys</c>; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="key"/> is <c>null</c>, empty, or consists only of white-space characters.
    /// </exception>
    /// <remarks>
    /// The key is split by dot ('.') characters and each part is used to traverse a hierarchical structure (assumed to be nested dictionaries).
    /// A key is considered valid only if each part exists and the final value is a <see cref="bool"/> explicitly set to <c>true</c>.
    /// Keys with a final value of <c>false</c>, <c>null</c>, or a non-boolean type are considered invalid.
    /// </remarks>
    private static bool IsValidConfigKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        var keyParts = key.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        object currentKeyValue = SupportedSettingKeys;

        for (var i = 0; i < keyParts.Length; i++)
        {
            var keyPart = keyParts[i];

            if (!TryGetNextLevelEntry(currentKeyValue, keyPart, out var nextLevelEntry))
            {
                return false;
            }

            if (IsLastConfigLevel(i) && nextLevelEntry is not true)
            {
                return false;
            }

            currentKeyValue = nextLevelEntry!;
        }

        return true;

        bool IsLastConfigLevel(int index) => index == keyParts.Length - 1;
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified <paramref name="key"/> from the given <paramref name="currentEntry"/>
    /// if it is a <see cref="Dictionary{TKey, TValue}"/> with string keys and object values.
    /// </summary>
    /// <param name="currentEntry">The object expected to be a <see cref="Dictionary{String, Object}"/> from which to retrieve the value.</param>
    /// <param name="key">The key whose value is to be retrieved from the dictionary.</param>
    /// <param name="entryValue">When this method returns, contains the value associated with the specified key if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the key was found in the dictionary; otherwise <c>false</c>.</returns>
    /// <remarks>This method returns <c>false</c> if <paramref name="currentEntry"/> is not a dictionary.</remarks>
    private static bool TryGetNextLevelEntry(object currentEntry, string key, out object? entryValue)
    {
        entryValue = null;

        if (currentEntry is not Dictionary<string, object> currentValues)
        {
            return false;
        }

        var result = currentValues.TryGetValue(key, out var entry);

        entryValue = entry!;

        return result;
    }
}

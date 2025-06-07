using System.Globalization;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConnyConsole.Settings;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Services;

public sealed class JsonConfigurationEditor(ILogger<JsonConfigurationEditor> logger, IFileSystem fileSystem) : IConfigurationEditor
{
    private const string ConfigFileTimeSpanFormat = @"d\.hh\:mm\:ss\.fff";

    /// <summary>
    /// Sets a configuration value in the JSON configuration file.
    /// </summary>
    /// <param name="settingKey">The hierarchical key path for the setting (e.g., "section.subsection.key").</param>
    /// <param name="newValue">The value to set for the specified setting key.</param>
    /// <exception cref="ArgumentException">Thrown when settingKey is null or whitespace.</exception>
    /// <exception cref="NotSupportedException">Thrown when settingKey is not supported.</exception>
    public void SetValue(string settingKey, string newValue)
    {
        IsSettingKeyValid(settingKey);

        var configFilePath = AppSettings.GetSystemConfigFilePath(fileSystem);
        EnsureDirectory(configFilePath);

        var root = LoadFile(configFilePath);
        var settingSection = NavigateToSettingSection(root, settingKey);

        var finalKeyPart = GetFinalKeyPart(settingKey);

        var parsedValue = ParseSettingValue(newValue);
        var currentValue = settingSection[finalKeyPart];

        settingSection[finalKeyPart] = parsedValue;

        SaveConfiguration(configFilePath, root);

        logger.LogInformation("Overwrote {SettingKey}={CurrentValue} with {ParsedValue}", settingKey, currentValue, parsedValue);
    }

    /// <summary>
    /// Validates the input setting key for null/whitespace and supported format.
    /// </summary>
    /// <param name="settingKey">The setting key to validate.</param>
    /// <exception cref="ArgumentException">Thrown when settingKey is null or whitespace.</exception>
    /// <exception cref="NotSupportedException">Thrown when settingKey is not supported.</exception>
    private static void IsSettingKeyValid(string settingKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingKey);

        if (!AppSettings.IsValidSettingKey(settingKey))
        {
            throw new NotSupportedException($"Setting key '{settingKey}' not supported.");
        }
    }

    /// <summary>
    /// Ensures that the directory for the specified file path exists. If the directory does not exist, it is created.
    /// </summary>
    /// <param name="filePath">
    /// The path of the file whose directory needs to be ensured. The directory of the specified path will be created if it doesn't exist.
    /// </param>
    /// <remarks>
    /// The method does not create the file itself; it only ensures the directory exists.
    /// </remarks>
    private void EnsureDirectory(string filePath)
    {
        var fileInfo = fileSystem.FileInfo.New(filePath);

        if (!fileInfo.Exists)
        {
            fileInfo.Directory?.Create();
        }
    }

    /// <summary>
    /// Loads a JSON file from the specified path and ensures it contains an "AppSettings" key with a valid <see cref="JsonObject"/>.
    /// If the file doesn't exist, it returns an empty JSON object with the "AppSettings" key instead.
    /// </summary>
    /// <param name="filePath">The path to the JSON file to load.</param>
    /// <returns>
    /// A <see cref="JsonObject"/> representing the content of the file. If the file does not exist or is invalid, an empty <see cref="JsonObject"/>
    /// with the "AppSettings" key is returned.
    /// </returns>
    private JsonObject LoadFile(string filePath)
    {
        var root = new JsonObject { [AppSettings.SectionName] = new JsonObject() };

        var fileInfo = fileSystem.FileInfo.New(filePath);

        if (!fileInfo.Exists)
        {
            return root;
        }

        var json = fileSystem.File.ReadAllText(filePath);
        root = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();

        // Ensure the "AppSettings" section exists
        if (!root.ContainsKey(AppSettings.SectionName) || root[AppSettings.SectionName] is not JsonObject)
        {
            root[AppSettings.SectionName] = new JsonObject();
        }

        return root;
    }

    /// <summary>
    /// Navigates through the JSON configuration hierarchy to locate the appropriate section for the setting.
    /// </summary>
    /// <param name="root">The root JSON node.</param>
    /// <param name="settingKey">The hierarchical settings key path.</param>
    /// <returns>The JSON object representing the section where the setting should be stored.</returns>
    private static JsonObject NavigateToSettingSection(JsonNode root, string settingKey)
    {
        var settingSection = root["AppSettings"]!.AsObject();
        var keyParts = settingKey.Split('.', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < keyParts.Length - 1; i++)
        {
            var part = keyParts[i];
            EnsureSettingSectionExists(settingSection, part);
            settingSection = settingSection[part]!.AsObject();
        }

        return settingSection;
    }

    /// <summary>
    /// Ensures that a specific section exists in the JSON configuration.
    /// Creates the section if it doesn't exist.
    /// </summary>
    /// <param name="section">The parent JSON section.</param>
    /// <param name="key">The key for the section to ensure.</param>
    private static void EnsureSettingSectionExists(JsonObject section, string key)
    {
        if (!section.ContainsKey(key) || section[key] is not JsonObject)
        {
            section[key] = new JsonObject();
        }
    }

    /// <summary>
    /// Extracts the final key part from the hierarchical setting key.
    /// </summary>
    /// <param name="settingKey">The full setting key path.</param>
    /// <returns>The last segment of the setting key path.</returns>
    private static string GetFinalKeyPart(string settingKey)
    {
        return settingKey.Split('.', StringSplitOptions.RemoveEmptyEntries)[^1];
    }

    /// <summary>
    /// Parses the input string value into the appropriate JSON type.
    /// Supports TimeSpan, integer, boolean, and string values.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>A JsonNode containing the parsed value in the appropriate type.</returns>
    private static JsonNode ParseSettingValue(string value)
    {
        if (DurationTimeParser.TryParse(value, out var timeSpanVal))
        {
            return timeSpanVal.ToString(ConfigFileTimeSpanFormat, CultureInfo.InvariantCulture);
        }

        if (int.TryParse(value, out var intVal))
        {
            return intVal;
        }

        if (bool.TryParse(value, out var boolVal))
        {
            return boolVal;
        }

        return value;
    }

    /// <summary>
    /// Saves the configuration to the specified file with proper formatting.
    /// </summary>
    /// <param name="configFile">The path to the configuration file.</param>
    /// <param name="root">The root JSON node containing the configuration.</param>
    private void SaveConfiguration(string configFile, JsonNode root)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };

        fileSystem.File.WriteAllText(configFile, root.ToJsonString(options));
    }
}

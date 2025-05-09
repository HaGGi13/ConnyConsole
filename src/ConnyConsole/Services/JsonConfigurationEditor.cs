using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConnyConsole.Settings;

namespace ConnyConsole.Services;

public sealed class JsonConfigurationEditor(IFileSystem fileSystem) : IConfigurationEditor
{
    private const string TimeSpanFormat = @"d\.hh\:mm\:ss\.fff";

    public void SetValue(string settingKey, string newValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingKey, nameof(settingKey));

        if (!AppSettings.IsValidSettingKey(settingKey))
        {
            throw new NotSupportedException($"Setting key '{settingKey}' not supported.");
        }

        var configFile = AppSettings.GetSystemConfigFilePath(fileSystem);
        EnsureDirectory(configFile);

        var root = LoadFile(configFile);
        var settingSection = root["AppSettings"]!.AsObject();

        var keyParts = settingKey.Split('.', StringSplitOptions.RemoveEmptyEntries);

        // Traverse the path except the last part (which is the final key)
        for (var i = 0; i < keyParts.Length - 1; i++)
        {
            var part = keyParts[i];

            if (!settingSection.ContainsKey(part) || settingSection[part] is not JsonObject)
            {
                settingSection[part] = new JsonObject();
            }

            settingSection = settingSection[part]!.AsObject();
        }

        var finalKeyPart = keyParts[^1];

        // Try to parse newValue into correct type (int, bool, etc.), else leave as string
        if (DurationTimeParser.TryParse(newValue, out var timeSpanVal))
        {
            settingSection[finalKeyPart] = timeSpanVal.ToString(TimeSpanFormat, CultureInfo.InvariantCulture);
        }
        else if (int.TryParse(newValue, out var intVal))
        {
            settingSection[finalKeyPart] = intVal;
        }
        else if (bool.TryParse(newValue, out var boolVal))
        {
            settingSection[finalKeyPart] = boolVal;
        }
        else
        {
            settingSection[finalKeyPart] = newValue;
        }

        Debug.WriteLine($"Set newValue: {settingKey}={settingSection[finalKeyPart]}");

        // Save
        fileSystem.File.WriteAllText(configFile, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
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
        var root = new JsonObject { ["AppSettings"] = new JsonObject() };

        var fileInfo = fileSystem.FileInfo.New(filePath);

        if (!fileInfo.Exists)
        {
            return root;
        }

        var json = fileSystem.File.ReadAllText(filePath);
        root = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();

        // Ensure "AppSettings" exists
        if (!root.ContainsKey("AppSettings") || root["AppSettings"] is not JsonObject)
        {
            root["AppSettings"] = new JsonObject();
        }

        return root;
    }
}

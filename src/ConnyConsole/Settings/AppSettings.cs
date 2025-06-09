using System.Text.Json.Serialization;

namespace ConnyConsole.Settings;

public sealed class AppSettings
{
    public const string SectionName = "AppSettings";

    private static readonly Dictionary<string, object> SupportedSettingKeys = new()
    {
        ["LoopOutputInterval"] = true,
        ["Cancellation"] = new Dictionary<string, object> { ["Timeout"] = true }
    };

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global -- used by startup configuration
    public TimeSpan LoopOutputInterval { get; set; } = TimeSpan.FromSeconds(1);

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global -- used by startup configuration
    [JsonPropertyName("Cancellation")]
    public CancellationSettings Cancellation { get; set; } = new();


    /// <summary>
    /// Determines whether the specified setting <paramref name="key"/> is valid and supported.
    /// </summary>
    /// <param name="key">The setting key to validate.</param>
    /// <returns><c>true</c> if the setting key is valid and supported; otherwise <c>false</c></returns>
    internal static bool IsValidSettingKey(string? key)
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
    /// The key is split by dot ('.') characters, and each part is used to traverse a hierarchical structure (assumed to be nested dictionaries).
    /// A key is considered valid only if each part exists and the final value is a <see cref="bool"/> explicitly set to <c>true</c>.
    /// Keys with a final value of <c>false</c>, <c>null</c>, or a non-boolean type are considered invalid.
    /// </remarks>
    private static bool IsValidConfigKey(string? key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        // Don't remove empty entries, this is handled by later code that checks against supported keys!
        var keyParts = key.Split('.', StringSplitOptions.TrimEntries);
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

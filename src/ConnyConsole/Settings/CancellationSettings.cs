using System.Text.Json.Serialization;

namespace ConnyConsole.Settings;

public class CancellationSettings
{
    public const string SectionName = "Cancellation";

    [JsonPropertyName("Timeout")] public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);
}

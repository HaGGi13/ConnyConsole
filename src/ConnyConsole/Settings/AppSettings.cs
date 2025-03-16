namespace ConnyConsole.Settings;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public TimeSpan LoopOutputInterval { get; init; }

    public TimeSpan CancellationTimeout { get; init; }
}

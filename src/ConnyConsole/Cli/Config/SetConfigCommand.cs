using System.CommandLine;
using ConnyConsole.Services;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Config;

public sealed class SetConfigCommand : Command
{
    private readonly IConfigurationEditor _configurationEditor;
    private readonly ILogger _logger;

    public SetConfigCommand(SettingKeyArgument settingKey, SettingValueArgument settingValue, IConfigurationEditor configurationEditor, ILogger<SetConfigCommand> logger)
        : base("set", "Sets a value for a configuration.")
    {
        AddArgument(settingKey);
        AddArgument(settingValue);

        _configurationEditor = configurationEditor;
        _logger = logger;

        this.SetHandler(Handle, settingKey, settingValue);
    }

    private void Handle(string key, string value)
    {
        try
        {
            _configurationEditor.SetValue(key, value);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
        }
    }
}

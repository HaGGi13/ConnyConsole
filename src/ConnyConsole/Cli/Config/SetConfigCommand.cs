using System.CommandLine;
using ConnyConsole.Services;
using ConnyConsole.Settings;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Config;

public sealed class SetConfigCommand : Command
{
    private readonly IConfigurationEditor _configurationEditor;
    private readonly ILogger _logger;

    public SetConfigCommand(SettingKeyArgument settingKey, SettingValueArgument settingValue, LocalOption localOption,
        GlobalOption globalOption, SystemOption systemOption, IConfigurationEditor configurationEditor,
        ILogger<SetConfigCommand> logger)
        : base("set", "Sets a configuration value for a defined configuration scope.")
    {
        AddArgument(settingKey);
        AddArgument(settingValue);

        AddOption(localOption); // is applicable, but anyway local is the default behavior, so it's ignored
        AddOption(globalOption);
        AddOption(systemOption);

        _configurationEditor = configurationEditor;
        _logger = logger;

        AddConfigurationScopeValidator(localOption, globalOption, systemOption);

        this.SetHandler(Handle, settingKey, settingValue, globalOption, systemOption);
    }

    /// <summary>
    /// Adds a validator to ensure that only one of the specified options is used at a time.
    /// </summary>
    /// <param name="options">The set of options to validate for exclusive usage.</param>
    private void AddConfigurationScopeValidator(params Option<bool>[] options)
    {
        this.AddValidator(result =>
        {
            var optionNames = options.Select(s => s.Name);
            var optionsUsedCount = result.Children.Count(s => optionNames.Contains(s.Symbol.Name));

            if (optionsUsedCount > 1)
            {
                result.ErrorMessage = "Only one configuration scope can be used at a time.";
            }
        });
    }

    private void Handle(string key, string value, bool isGlobal, bool isSystem)
    {
        // Local is the default behavior and overridden by global or system, if provided
        var scope = ConfigurationScope.Local;

        if (isGlobal)
        {
            scope = ConfigurationScope.Global;
        }
        else if (isSystem)
        {
            scope = ConfigurationScope.System;
        }

        try
        {
            var resultMessage = _configurationEditor.SetValue(key, value, scope);
            _logger.LogInformation("Set setting result: {Message}", resultMessage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Set setting error occured: {ErrorMessage}", e.Message);
        }
    }
}

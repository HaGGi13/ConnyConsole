using System.CommandLine;
using System.CommandLine.Parsing;
using ConnyConsole.Services;
using ConnyConsole.Settings;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Config;

public sealed partial class SetConfigCommand : Command
{
    private readonly IConfigurationEditor _configurationEditor;
    private readonly ILogger _logger;

    public SetConfigCommand(SettingKeyArgument settingKey, SettingValueArgument settingValue, LocalOption localOption,
        GlobalOption globalOption, SystemOption systemOption, IConfigurationEditor configurationEditor,
        ILogger<SetConfigCommand> logger)
        : base("set", "Sets a configuration value for a defined configuration scope.")
    {
        Arguments.Add(settingKey);
        Arguments.Add(settingValue);

        Options.Add(localOption); // is applicable, but anyway local is the default behavior, so it's ignored
        Options.Add(globalOption);
        Options.Add(systemOption);

        _configurationEditor = configurationEditor;
        _logger = logger;

        Validators.Add(result => ConfigurationScopeValidator(result, localOption, globalOption, systemOption));

        SetAction(result => Handle(result, settingKey, settingValue));
    }

    /// <summary>
    /// Ensures that only one configuration scope option (e.g., local, global, or system)
    /// is specified at any given time when executing the command.
    /// </summary>
    /// <param name="commandResult">The result of the executed command, containing the parsed options.</param>
    /// <param name="mutexedConfigScopeOptions">The set of configuration scope options to validate for mutual exclusivity.</param>
    private static void ConfigurationScopeValidator(CommandResult commandResult, params Option[] mutexedConfigScopeOptions)
    {
        var hasConflictingScopes = mutexedConfigScopeOptions.Count(o => commandResult.GetResult(o) is not null) > 1;

        if (hasConflictingScopes)
        {
            commandResult.AddError("Only one configuration scope can be used at a time.");
        }
    }

    /// <summary>
    /// Handles the logic for setting a configuration value based on the provided command-line arguments
    /// and options, including determining the appropriate configuration scope.
    /// </summary>
    /// <param name="parseResult">The result of parsing the command-line input, containing argument and option values.</param>
    /// <param name="keyArgument">The argument representing the configuration key to be set.</param>
    /// <param name="valueArgument">The argument representing the value to assign to the configuration key.</param>
    private void Handle(ParseResult parseResult, SettingKeyArgument keyArgument, SettingValueArgument valueArgument)
    {
        try
        {
            var key = parseResult.GetRequiredValue(keyArgument);
            var value = parseResult.GetRequiredValue(valueArgument);

            var scope = GetConfigurationScope(parseResult);

            var resultMessage = _configurationEditor.SetValue(key, value, scope);
            LogSetSettingResultMessage(resultMessage);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Set setting error occured: {ErrorMessage}", exception.Message);
        }
    }

    /// <summary>
    /// Determines the configuration scope (local, global, or system) based on the parsed command-line arguments.
    /// </summary>
    /// <param name="parseResult">The result from parsing the command-line input, containing options and arguments.</param>
    /// <returns>The configuration scope to be used, defaulting to local if neither global nor system is specified.</returns>
    private ConfigurationScope GetConfigurationScope(ParseResult parseResult)
    {
        // Local is the default behavior and overridden by global or system, if provided
        var scope = ConfigurationScope.Local;

        var isGlobal = parseResult.GetValue(Options.OfType<GlobalOption>().First());
        var isSystem = parseResult.GetValue(Options.OfType<SystemOption>().First());

        if (isGlobal)
        {
            scope = ConfigurationScope.Global;
        }
        else if (isSystem)
        {
            scope = ConfigurationScope.System;
        }

        return scope;
    }
}

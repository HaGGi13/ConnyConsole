using Serilog;
using Serilog.Formatting.Display;

namespace ConnyConsole.Extensions;

public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Extends <see cref="LoggerConfiguration" /> by a default console logger configuration.
    /// </summary>
    /// <param name="loggerConfiguration">The existing logger configuration to extend.</param>
    /// <param name="messageTemplate">The log message template to use.</param>
    /// <returns>The logger configuration extended by a default console logger configuration.</returns>
    public static LoggerConfiguration AddDefaultConsoleLogger(this LoggerConfiguration loggerConfiguration,
        string messageTemplate = "{Message:lj}{NewLine}") =>
        loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.Console(new MessageTemplateTextFormatter(messageTemplate));
}

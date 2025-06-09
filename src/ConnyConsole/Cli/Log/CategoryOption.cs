using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Log;

public sealed class CategoryOption : Option<LogLevel>
{
    public CategoryOption() : base(name: "--category",
        description: "The log category.",
        getDefaultValue: () => LogLevel.Information)
    {
        AddAlias("-c");
    }
}

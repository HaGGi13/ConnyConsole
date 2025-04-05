using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Options;

public class CategoryOption : Option<LogLevel>
{
    public CategoryOption() : base(name: "--cat",
        description: "The log category.",
        getDefaultValue: () => LogLevel.Information)
    {
        AddAlias("-c");
    }
}

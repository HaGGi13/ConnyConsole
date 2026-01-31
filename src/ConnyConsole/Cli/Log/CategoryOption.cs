using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Cli.Log;

public sealed class CategoryOption : Option<LogLevel>
{
    public CategoryOption() : base("--category", "-c")
    {
        Description = "The log category.";
        DefaultValueFactory = _ => LogLevel.Information;
    }
}

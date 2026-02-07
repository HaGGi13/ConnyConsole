using System.Text;
using Microsoft.Extensions.Configuration;

namespace ConnyConsole.Tests.TestHelpers;

public static class TestConfiguration
{
    public const string ConfigurationWithEmptySerilog =
        """
        {
          "Serilog": {
          },
          "AppSettings": {
            "LoopOutputInterval": "00:00:02",
            "CancellationTimeout": "00:00:03"
          }
        }
        """;

    public const string ConfigurationWithoutSerilog =
        """
        {
          "AppSettings": {
            "LoopOutputInterval": "00:00:02",
            "CancellationTimeout": "00:00:03"
          }
        }
        """;

    private const string CompleteConfiguration =
        """
        {
          "Serilog": {
            "MinimumLevel": {
              "Default": "Information",
              "Override": {
                "Microsoft": "Warning",
                "Microsoft.Hosting.Lifetime": "Information",
                "Microsoft.DbLoggerCategory.Database.Command": "Information"
              }
            },
            "Using": [
              "Serilog.Sinks.File",
              "Serilog.Sinks.Console"
            ],
            "WriteTo": [
              {
                "Name": "File",
                "Args": {
                  "path": "Logs/On-.log",
                  "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                  "rollingInterval": "Day"
                }
              },
              {
                "Name": "Console",
                "Args": {
                  "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                }
              }
            ],
            "Properties": {
              "Application": "ConnyConsole"
            }
          },
          "AppSettings": {
            "LoopOutputInterval": "00:00:02",
            "CancellationTimeout": "00:00:03"
          }
        }
        """;

    public static IConfigurationRoot GetConfiguration(string jsonConfiguration = CompleteConfiguration) =>
        new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(jsonConfiguration)))
            .Build();
}

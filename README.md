[![Build and analyze][50]][70]
[![Quality Gate Status][51]][71]

# ConnyConsole

ConnyConsole is a console CLI project that uses `System.CommandLine` from Microsoft for argument parsing to collect some experience with this library.

## Table of content

- [ConnyConsole](#connyconsole)
    - [Table of content](#table-of-content)
    - [Features](#features)
        - [Business code](#business-code)
        - [Test code](#test-code)
        - [Pipeline](#pipeline)
    - [References/documentation:](#referencesdocumentation)

## Features

This chapter gives a rough overview of what was implemented as an example implementation.

### Business code

Please note that some the features listed are based on or inspired by the article series [A Beginner's Guide to .NET's HostBuilder: Part 1 and following][5].

- Console startup implemented with **[Host.CreateDefaultBuilder][6]** that enables/contains the following:
    - **Dependency injection** configuration via [`ConfigureServices`][7] and extension method to keep `Program.cs` simple;
        - Own extension method `AddConfiguration` registers all dependencies incl.:
            - Logger configuration with injected configuration;
            - Configuration registered for [options pattern][8];
    - **Loads configuration from specific subdirectory** `Config` that contains `appsettings.json` and `appsettings.Development.json`:
        - Current environment resolved from injected [`HostBuilderContext`][9] to use related `appsettings.json` file;
        - `appsettings` files intentionally in subdirectory `Config` to have to define them specifically for loading, to cover a case as example;
- **[Serilog][10]** set up as a logger:
    - Startup-logger and injectable logger based on configuration files;
    - Logging into on console and file in defined format;
    - Serilog can throw strange/not relatable exceptions during configuration; when JSON config is wrong, such exceptions will be printed on the console;
- Current _dummy_ logic executed async with **graceful and enforceable cancellation**;
    - First `[Ctrl] + [C]` or `[Ctrl] + [Break]` initiates graceful cancellation;
        - Waits until logic finishes or a configurable timeout reaches and closes the application;
    - Second `[Ctrl] + [C]` or `[Ctrl] + [Break]` initiates immediate enforced cancellation;
        - Application exists immediately;
    - All that magic happens in [`ConsoleCancellationTokenSource`][11] class;
    - Console cancellation event is registered in [`App`][12] class;
- Console application icon defined (check `*.csproj` file);

### Test code

As an application is only as good as it was tested, this chapter gives some insights into how the console application tests were implemented.

- Unit tests are implemented with the following libraries/frameworks:
    - [AutoFixture.AutoNSubstitute][15]
    - [AwesomeAssertions][16] (fully community-driven fork of _FluentAssertions_)
    - [NSubstitute][17]
    - [xUnit][14]
- Graceful + enforced cancellation are tested with simulated `[Ctrl] + [C]` keys pressed ([`ConsoleCancellationTokenSourceTests.cs`][13]);
- Dependency injection extension method incl. lifetime checks;
    - Lifetime check helps to notice fast if a lifetime was changed by accident or just to highlight that it was changed in general;
- Async dummy logic execution;

### Pipeline

This chapter provides an overview of what the current build pipeline provides.

- [GitVersion][18] integrated for auto [SemVer (Semantic Versioning)][19] based on Git history;
- [SonarQube][20] (cloud, free plan) integrated;
- Build console application with `Release` configuration;
- Run tests;
    - Collect test run results as `*.trx` file;
    - Collect code coverage in `OpenCover` format, later on used to publish on SonarQube;
        - Done by using [coverlet.msbuild + coverlet.collector][21] (_in test project only_);
    - Both files, the coverage file and test result file, are published as build artifacts;
    - Passed, failed and skipped tests listed as part of run summary, realized with [`GitHubActionsTestLogger`][22] package;

## References/documentation:

The following are some used articles listed.
- [Parse the Command Line with System.CommandLine][1]
- [System.CommandLine overview][2]
- [System.CommandLine on GitHub][3]
- [Tutorial: Get started with System.CommandLine][4]
- [A Beginner's Guide to .NET's HostBuilder: Part 1 and following][5]

<!--# Not Markdown compliant, but the only proper working solution to center content on GitHub -->
<div align="center">
  <a href="https://sonarcloud.io/summary/overall?id=HaGGi13_ConnyConsole" target="_blank" title="Overall code analysis overview">
    <img src="https://sonarcloud.io/images/project_badges/sonarcloud-light.svg" alt="Overall code analysis badge" title="Scanned by SonarQube badge"/>
  </a>
</div>

<!--# references -->
[1]: https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline "Parse the Command Line with System.CommandLine"
[2]: https://learn.microsoft.com/en-us/dotnet/standard/commandline/ "System.CommandLine overview"
[3]: https://github.com/dotnet/command-line-api "GitHub: dotnet > command-line-api"
[4]: https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial "Tutorial: Get started with System.CommandLine"
[5]: https://medium.com/@sawyer.watts/a-beginners-guide-to-net-s-hostbuilder-part-0-78882aab60f8 "Medion: HostBuilder article series"
[6]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.host.createdefaultbuilder?view=net-9.0-pp#microsoft-extensions-hosting-host-createdefaultbuilder "MS Docs: Host.CreateDefaultBuilder Method"
[7]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostbuilder.configureservices?view=net-9.0-pp&viewFallbackFrom=net-9.0 "MS Docs: IHostBuilder.ConfigureServices Method"
[8]: https://learn.microsoft.com/en-us/dotnet/core/extensions/options "MS Docs: Options pattern in .NET"
[9]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostbuildercontext?view=net-9.0-pp "MS Docs: HostBuilderContext Class"
[10]: https://serilog.net/ "Serilog web site"
[11]: src/ConnyConsole/Infrastructure/ConsoleCancellationTokenSource.cs "Graceful + enforced cancellation logic"
[12]: src/ConnyConsole/App.cs#L41 "Console cancellation event registration"
[13]: tests/ConnyConsole.Tests/ConsoleCancellationTokenSourceTests.cs "Cancellation unit tests"
[14]: https://xunit.net/ "xUnit web site"
[15]: https://github.com/AutoFixture/AutoFixture?tab=readme-ov-file#mocking-libraries "GitHub: AutoFixture > Mocking libraries"
[16]: https://github.com/AwesomeAssertions/AwesomeAssertions "GitHub: AwesomeAssertions"
[17]: https://nsubstitute.github.io/ "NSubstitute web site"
[18]: https://gitversion.net/docs/ "GitVersion documentation"
[19]: https://semver.org/ "Semantic Versioning 2.0.0 documentation"
[20]: https://www.sonarsource.com/products/sonarcloud/ "SonarQube Cloud product web site"
[21]: https://github.com/coverlet-coverage/coverlet "GitHub: Coverlet repository"
[22]: https://github.com/Tyrrrz/GitHubActionsTestLogger "GitHub: GitHubActionsTestLogger repository"

<!--# badge image references -->
[50]: https://github.com/HaGGi13/ConnyConsole/actions/workflows/build-connyconsole.yaml/badge.svg
[51]: https://sonarcloud.io/api/project_badges/measure?project=HaGGi13_ConnyConsole&metric=alert_status

<!--# badge link references -->
[70]: https://github.com/HaGGi13/ConnyConsole/actions/workflows/build-connyconsole.yaml "Build pipeline"
[71]: https://sonarcloud.io/summary/new_code?id=HaGGi13_ConnyConsole "Latest new code analysis"

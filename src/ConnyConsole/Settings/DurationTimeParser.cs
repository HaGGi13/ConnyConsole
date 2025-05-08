using System.Globalization;
using System.Text.RegularExpressions;

namespace ConnyConsole.Settings;

public static partial class DurationTimeParser
{
    [GeneratedRegex(@"^(?:(?<d>\d+)\.)?(?<hh>[01]\d|2[0-3]):(?<mm>[0-5]\d):(?<ss>[0-5]\d)(?:\.(?<ms>\d{0,3}))?$" +
                    @"|^(?:(?<days>\d+(?:\.\d+)?)\s*(d|day|days)\b)?\s*" +
                    @"(?:(?<hours>\d+(?:\.\d+)?)\s*(h|hour|hours)\b)?\s*" +
                    @"(?:(?<minutes>\d+(?:\.\d+)?)\s*(m|min|minute|minutes)\b)?\s*"+
                    @"(?:(?<seconds>\d+(?:\.\d+)?)\s*(s|sec|second|seconds)\b)?\s*" +
                    @"(?:(?<milliseconds>\d+)\s*(ms|millisecond|milliseconds)\b)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CombinedTimeMyRegex();

    private static readonly Regex TimeRegex = CombinedTimeMyRegex();

    public static bool TryParse(string? input, out TimeSpan timeSpan)
    {
        timeSpan = TimeSpan.Zero;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        input = input.Trim();

        var match = TimeRegex.Match(input);
        if (!match.Success)
        {
            return false;
        }

        return TryParseDurationFormat(match, out timeSpan) ||
               TryParseHumanReadableDurationFormat(match, out timeSpan);
    }

    /// <summary>
    /// Tries to parse a possibly matched time in duration format [d.]HH:mm:ss[.fff].
    /// </summary>
    /// <param name="match">Possibly RegEx match that contains values to process.</param>
    /// <param name="result">The parsed result object if successfully parsed; otherwise <see cref="TimeSpan.Zero"/>.</param>
    /// <returns><c>true</c> if successfully parsed; otherwise <c>false</c></returns>
    private static bool TryParseDurationFormat(Match match, out TimeSpan result)
    {
        result = TimeSpan.Zero;

        if (!match.Groups["hh"].Success)
        {
            return false;
        }

        _ = int.TryParse(match.Groups["d"].Value, out var d);
        _ = int.TryParse(match.Groups["hh"].Value, out var hh);
        _ = int.TryParse(match.Groups["mm"].Value, out var mm);
        _ = int.TryParse(match.Groups["ss"].Value, out var ss);
        _ = int.TryParse(match.Groups["ms"].Value, out var ms);

        result = new TimeSpan(d, hh, mm, ss, ms);

        return true;
    }

    /// <summary>
    /// Parses a possibly matched time in human-readable duration format. For instance like "1h 13m 2s", "5 sec" or "10min 30s".
    /// </summary>
    /// <param name="match">Possibly RegEx match that contains values to process.</param>
    /// <param name="result">The parsed result object if successfully parsed; otherwise <see cref="TimeSpan.Zero"/>.</param>
    /// <returns>The parsed result object.</returns>
    private static bool TryParseHumanReadableDurationFormat(Match match, out TimeSpan result)
    {
        result = TimeSpan.Zero;

        try
        {
            var days = match.Groups["days"].Success
                ? double.Parse(match.Groups["days"].Value, CultureInfo.InvariantCulture)
                : 0;

            var hours = match.Groups["hours"].Success
                ? double.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture)
                : 0;

            var minutes = match.Groups["minutes"].Success
                ? double.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture)
                : 0;

            var seconds = match.Groups["seconds"].Success
                ? double.Parse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture)
                : 0;

            var milliSeconds = match.Groups["milliseconds"].Success
                ? int.Parse(match.Groups["milliseconds"].Value, CultureInfo.InvariantCulture)
                : 0;

            var timeSpan = TimeSpan.FromDays(days) +
                           TimeSpan.FromHours(hours) +
                           TimeSpan.FromMinutes(minutes) +
                           TimeSpan.FromSeconds(seconds) +
                           TimeSpan.FromMilliseconds(milliSeconds);

            // As we do not support nanoseconds or ticks, we have to round to the nearest millisecond
            result = RoundOnMilliseconds(timeSpan);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Rounds a given <see cref="TimeSpan"/> to the nearest milliseconds with a precision of 1 millisecond.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> instance to round.</param>
    /// <returns>The new rounded <see cref="TimeSpan"/> instance.</returns>
    private static TimeSpan RoundOnMilliseconds(TimeSpan timeSpan)
    {
        var roundedTicks =
            (long)(Math.Round((double)timeSpan.Ticks / TimeSpan.TicksPerMillisecond) * TimeSpan.TicksPerMillisecond);

        return new TimeSpan(roundedTicks);
    }
}

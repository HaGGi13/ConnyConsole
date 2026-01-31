using AwesomeAssertions;
using ConnyConsole.Settings;

namespace ConnyConsole.Tests.Settings;

public class AppSettingsTests
{
    [Theory]
    [InlineData("LoopOutputInterval", true)]
    [InlineData("Cancellation.Timeout", true)]
    public void IsValidSettingKey_ValidKeys_ReturnsTrue(string key, bool expected)
    {
        // Act
        var result = AppSettings.IsValidSettingKey(key);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("LoopOutputInterval.")]
    [InlineData(".Timeout")]
    [InlineData("Cancellation..Timeout")]
    [InlineData("InvalidKey")]
    [InlineData("Cancellation")] // Looks valid but is an object that cannot hold a simple value (not marked as a last level in supported keys)
    [InlineData("Cancellation.InvalidSubKey")]
    [InlineData("LoopOutputInterval.InvalidNesting")]
    [InlineData("Cancellation.Timeout.TooDeep")]
    public void IsValidSettingKey_InvalidKeys_ReturnsFalse(string key)
    {
        // Act
        var result = AppSettings.IsValidSettingKey(key);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void IsValidSettingKey_NullOrWhiteSpace_ThrowsArgumentException(string? key)
    {
        // Act
        var act = () => AppSettings.IsValidSettingKey(key);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}

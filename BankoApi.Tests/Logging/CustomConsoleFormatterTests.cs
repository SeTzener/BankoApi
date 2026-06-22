using System.Text.Json;
using BankoApi.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankoApi.Tests.Logging;

public class CustomConsoleFormatterTests
{
    private static readonly CustomConsoleFormatter Formatter = new();

    [Theory]
    [InlineData(LogLevel.Critical, "💥")]
    [InlineData(LogLevel.Error, "❌")]
    [InlineData(LogLevel.Warning, "⚠️")]
    [InlineData(LogLevel.Information, "ℹ️")]
    [InlineData(LogLevel.Debug, "🔍")]
    [InlineData(LogLevel.Trace, "🔬")]
    public void Write_IncludesCorrectEmojiForLevel(LogLevel level, string emoji)
    {
        var output = WriteEntry(level, "test message", null);
        Assert.Contains(emoji, output);
    }

    [Fact]
    public void Write_IncludesTimestamp()
    {
        var before = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var output = WriteEntry(LogLevel.Error, "test", null);
        var after = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var timestamp = output[1..output.IndexOf(']')];

        Assert.True(string.Compare(timestamp, before) >= 0);
        Assert.True(string.Compare(timestamp, after) <= 0);
    }

    [Fact]
    public void Write_IncludesLogLevel()
    {
        var output = WriteEntry(LogLevel.Error, "test", null);
        Assert.Contains("ERROR", output);
    }

    [Fact]
    public void Write_IncludesMessage()
    {
        var output = WriteEntry(LogLevel.Error, "hello world", null);
        Assert.Contains("hello world", output);
    }

    [Fact]
    public void Write_WhenExceptionIsNull_DoesNotIncludeStackTrace()
    {
        var output = WriteEntry(LogLevel.Error, "test", null);
        Assert.DoesNotContain("Stack:", output);
    }

    [Fact]
    public void Write_WhenExceptionIsNotNull_IncludesExceptionMessage()
    {
        var exception = new InvalidOperationException("Something went wrong");
        var output = WriteEntry(LogLevel.Error, "test", exception);
        Assert.Contains("Stack:", output);
        Assert.Contains("Something went wrong", output);
    }

    private static string WriteEntry(LogLevel level, string message, Exception? exception)
    {
        var writer = new StringWriter();
        var logEntry = new LogEntry<string>(
            logLevel: level,
            category: "Test",
            eventId: default,
            state: message,
            exception: exception,
            formatter: (s, _) => s ?? ""
        );
        Formatter.Write(in logEntry, null, writer);
        return writer.ToString();
    }
}

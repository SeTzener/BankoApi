using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace BankoApi.Logging;

public class CustomConsoleFormatter : ConsoleFormatter
{
    public CustomConsoleFormatter() : base("Custom") { }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var level = logEntry.LogLevel;
        var emoji = GetEmoji(level);
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        var exception = logEntry.Exception;

        textWriter.Write($"[{timestamp}] {level.ToString().ToUpperInvariant()}: {emoji} {message}");
        if (exception != null)
        {
            textWriter.Write($"\n    Stack: {exception.Message}");
        }

        textWriter.WriteLine();
    }

    private static string GetEmoji(LogLevel level) => level switch
    {
        LogLevel.Critical => "💥",
        LogLevel.Error => "❌",
        LogLevel.Warning => "⚠️",
        LogLevel.Information => "ℹ️",
        LogLevel.Debug => "🔍",
        LogLevel.Trace => "🔬",
        _ => ""
    };
}

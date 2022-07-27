using System.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace BudgetReview;

/// <summary>
/// Wraps another logger and prepends a specified prefix to every message.
/// </summary>
public class PrefixedLogger : ILogger
{
    private readonly ILogger parentLogger;
    private readonly string prefix;
    private readonly MessageTemplateParser messageTemplateParser = new ();

    /// <summary>
    /// Creates a logger that prefixes all messages sent to it and forwards them to the given parent logger.
    /// </summary>
    /// <remarks>
    /// Automatically includes a colon and space with the prefix. For example, "Prefix: Original message".
    /// </remarks>
    public PrefixedLogger(ILogger parentLogger, string prefix)
    {
        this.parentLogger = parentLogger;
        this.prefix = prefix;
    }

    void ILogger.Write(LogEvent logEvent)
    {
        var newText = $"{prefix}: {logEvent.MessageTemplate.Text}";
        var messageTemplate = messageTemplateParser.Parse(newText);

        var properties = logEvent.Properties
            .Select(pair => new LogEventProperty(pair.Key, pair.Value));
        var prefixedEvent = new LogEvent(
            logEvent.Timestamp,
            logEvent.Level,
            logEvent.Exception,
            messageTemplate,
            properties);

        parentLogger.Write(prefixedEvent);
    }
}

using LlamachantFramework.Workflow.Logging;
using LlamaLogger.Core.Extensions;

namespace Llamachant.ExpressApp.Demo.WorkflowService.Utils;

public class LlamaLoggerConsoleLogger : ILogController
{
    LlamaLogger.Core.LlamaLoggerClient? client;

    public LlamaLoggerConsoleLogger(string apikey, string version)
    {
        if (!string.IsNullOrEmpty(apikey) && !apikey.Equals("APIKey", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(version))
            client = new LlamaLogger.Core.LlamaLoggerClient(apikey, version);
    }

    public void Dispose()
    {
        if (client != null)
        {
            client.FlushQueue(20000);
            client.Dispose();
        }

        client = null;
    }

    public void LogError(Exception ex)
    {
        if (client != null)
            client.LogError(ex);

        LogText(ex.GetFullExceptionText(), LoggingLevel.Error);
    }

    public void LogWarning(string text)
    {
        if (client != null)
            client.LogWarning(text);

        LogText(text, LoggingLevel.Warning);
    }

    public void LogWarning(Exception ex)
    {
        if (client != null)
            client.LogWarning(ex);

        LogText(ex.GetFullExceptionText(), LoggingLevel.Warning);
    }

    public void LogDiagnostic(string text) => LogText(text, LoggingLevel.Diagnostic);
    public void LogInfo(string text) => LogText(text, LoggingLevel.Info);
    public void LogRequired(string text) => LogText(text, LoggingLevel.Required);

    public void LogText(string text, LoggingLevel level)
    {
        var originalforeground = Console.ForegroundColor;

        Dictionary<LoggingLevel, ConsoleColor> colors = new Dictionary<LoggingLevel, ConsoleColor>()
        {
            { LoggingLevel.Diagnostic, ConsoleColor.Gray  },
            { LoggingLevel.Info, ConsoleColor.White  },
            { LoggingLevel.Warning, ConsoleColor.Yellow },
            { LoggingLevel.Error, ConsoleColor.Red },
            { LoggingLevel.Required, ConsoleColor.Cyan }
        };

        Console.ForegroundColor = colors[level];

        Console.WriteLine($"[{DateTime.Now.ToString("G")}] {text}");

        Console.ForegroundColor = originalforeground;
    }
}

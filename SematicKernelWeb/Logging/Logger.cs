using System.Diagnostics.CodeAnalysis;
using SematicKernelWeb.Classes;

namespace SematicKernelWeb.Logging;

public class Logger : ILogger
{
    protected readonly LoggerProvider LoggerProvider;


    public Logger([NotNull] LoggerProvider provider)
    {
        LoggerProvider = provider;
        LoggerProvider.LogDirectory = Config.Instance.LogFilePath;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (logLevel <= Config.Instance.CurrentLogLevel)
            return;

        //if (!IsEnabled(logLevel))
        //    return;

        string logRecord =
            $"{"[" + DateTimeOffset.Now.ToString("G") + "]"} [{logLevel}] {formatter(state, exception)} {(exception != null ? exception.StackTrace : "")}";
        LogRecord rec = new(DateTime.Now, logLevel, logRecord);
        LoggerProvider.AddRecord(rec);
    }
}
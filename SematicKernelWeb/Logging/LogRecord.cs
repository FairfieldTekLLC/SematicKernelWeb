namespace SematicKernelWeb.Logging;

public class LogRecord
{
    public LogRecord(DateTime eventDate, LogLevel logLevel, string message)
    {
        EventDate = eventDate;
        LogLevel = logLevel;
        Message = message;
    }

    public DateTime EventDate { get; set; }
    public LogLevel LogLevel { get; set; }
    public string Message { get; set; }

    public override string ToString()
    {
        return Message;
    }
}
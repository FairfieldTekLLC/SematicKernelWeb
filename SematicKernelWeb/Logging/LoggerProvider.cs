using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text;
using System.Timers;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace SematicKernelWeb.Logging;

[ProviderAlias("LoggerProvider")]
public class LoggerProvider : ILoggerProvider
{
    private const long MaxFileSize = 1024 * 1000; //500 megs per file.

    //@"C:\EarsLogs";
    private const int MaxNumberOfLogFiles = 5;
    public static string LogDirectory = string.Empty;

    private static readonly ConcurrentBag<string> AllLogFiles = new();
    private static readonly ConcurrentQueue<string> ActiveLogFiles = new();
    private static List<LogRecord> _log = new();
    private static FileStream _fileStream;

    private static string _lastLogFileName = string.Empty;
    private static string _lastLine = string.Empty;

    private static readonly UnicodeEncoding UniEncoding = new();

    private static bool _writing;

    private static readonly ConcurrentQueue<byte[]> ToWrite = new();
    private static Timer _writeTimer;


    // ReSharper disable once UnusedParameter.Local
    public LoggerProvider(IOptions<LoggingOptions> options)
    {
        _writeTimer = new Timer(10);
        _writeTimer.Elapsed += WriteTimer_Elapsed;
        _writeTimer.Start();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new Logger(this);
    }

    public void Dispose()
    {
        _writeTimer?.Stop();
        _writeTimer?.Dispose();
        _fileStream?.Close();
        _fileStream?.Dispose();
    }

    public void AddRecord(LogRecord record)
    {
        if (record == null)
            return;

        lock (AllLogFiles)
        {
            if (_lastLine.Equals(record.Message, StringComparison.InvariantCultureIgnoreCase))
                return;
            _lastLine = record.Message;
            ToWrite.Enqueue(UniEncoding.GetBytes($"{record.EventDate} \t {record.LogLevel} \t {record.Message}\r\n"));
            _log.Insert(0, record);
            if (_log.Count > 5000)
                _log = _log.GetRange(0, 4500);
        }
    }

    private static string NewLog()
    {
        int attempt = 0;
        if (!Directory.Exists(LogDirectory))
            Directory.CreateDirectory(LogDirectory);
        string newFileName;
        while (true)
        {
            string fileName = "WebServer." + DateTime.Now.ToString("yyyyMMdd") + "-" +
                              attempt.ToString().PadLeft(4, '0') + ".log";
            attempt++;
            if (attempt == int.MaxValue)
            {
                attempt = 0;
                fileName = "WebServer." + DateTime.Now.ToString("yyyyMMdd") + "-" + attempt.ToString().PadLeft(4, '0') +
                           ".log";
                AllLogFiles.Clear();
                foreach (string file in ActiveLogFiles)
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception)
                    {
                        //Doesn't matter
                    }
            }

            newFileName = Path.Combine(LogDirectory, fileName);
            if (File.Exists(newFileName))
            {
                if (new FileInfo(newFileName).Length < MaxFileSize)
                    break;
            }
            else
            {
                if (!AllLogFiles.ToArray().Contains(newFileName))
                    break;
            }
        }

        if (!AllLogFiles.Contains(newFileName))
            AllLogFiles.Add(newFileName);
        if (!ActiveLogFiles.Contains(newFileName))
            ActiveLogFiles.Enqueue(newFileName);

        if (ActiveLogFiles.Count <= MaxNumberOfLogFiles)
            return newFileName;

        if (!ActiveLogFiles.TryDequeue(out string fn)) return newFileName;
        try
        {
            File.Delete(fn);
        }
        catch (Exception)
        {
            //Doesn't matter
        }

        return newFileName;
    }

    public IReadOnlyCollection<LogRecord> RetrieveLog()
    {
        return new ReadOnlyCollection<LogRecord>(_log);
    }


    private static void WriteTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        lock (_writeTimer)
        {
            if (_writing)
                return;
            _writing = true;
            _writeTimer.Stop();
        }

        lock (_lastLogFileName)
        {
            while (true)
                try
                {
                    _lastLogFileName = NewLog();
                    _fileStream = File.Open(_lastLogFileName, FileMode.OpenOrCreate);
                    _fileStream.Seek(0, SeekOrigin.End);
                    break;
                }
                catch (Exception)
                {
                    try
                    {
                        _fileStream?.Close();
                    }
                    catch (Exception)
                    {
                        //
                    }

                    try
                    {
                        _fileStream?.Dispose();
                    }
                    catch (Exception)
                    {
                        //
                    }
                }

            int counter = 0;
            while (ToWrite.TryDequeue(out byte[] data))
            {
                _fileStream.Write(data);
                counter++;
                if (counter > 50)
                    break;
            }

            try
            {
                _fileStream?.Close();
                _fileStream?.Dispose();
                _fileStream = null;
            }
            catch (Exception)
            {
                //
            }
        }

        lock (_writeTimer)
        {
            _writing = false;
            _writeTimer.Start();
        }
    }
}
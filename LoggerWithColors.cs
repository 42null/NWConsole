using NLog;
using NLog.Fluent;

public class LoggerWithColors
{
    public const bool IS_UNIX = true;
    static string loggerPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "nlog.config";
    static string readWriteFilePath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "Tickets.csv";

    static NLog.Logger originalLogger;

    public LoggerWithColors()
    {
        originalLogger = LogManager.Setup().LoadConfigurationFromFile(loggerPath).GetCurrentClassLogger();
    }


    public void Trace(string message)
    {
        applyOriginalButWithColors(message, UserInteractions.traceColor, Logger_Type.Trace);
    }

    public void Debug(string message)
    {
        applyOriginalButWithColors(message, UserInteractions.debugColor, Logger_Type.Debug);
    }

    public void Info(string message)
    {
        applyOriginalButWithColors(message, UserInteractions.infoColor, Logger_Type.Info);
    }

    public void Warn(string message)
    {
        applyOriginalButWithColors(message, UserInteractions.warnColor, Logger_Type.Warn);
    }

    public void Error(string message)
    {
        applyOriginalButWithColors(message, UserInteractions.errorColor, Logger_Type.Error);
    }

    public void Fatal(string message)
    {
        applyOriginalButWithColors(message, UserInteractions.fatalColor, Logger_Type.Fatal);
    }


    private void applyOriginalButWithColors(string message, ConsoleColor newColor, Logger_Type type)
    {
        ConsoleColor temporaryStoreBeforeColor = Console.ForegroundColor;
        Console.ForegroundColor = newColor;

        switch (type)
        {
            case Logger_Type.Trace:
                originalLogger.Trace(message);
                break;
            case Logger_Type.Debug:
                originalLogger.Debug(message);
                break;
            case Logger_Type.Info:
                originalLogger.Info(message);
                break;
            case Logger_Type.Warn:
                originalLogger.Warn(message);
                break;
            case Logger_Type.Error:
                originalLogger.Error(message);
                break;
            case Logger_Type.Fatal:
                originalLogger.Fatal(message);
                break;
        }
        Console.ForegroundColor = temporaryStoreBeforeColor;
    }


    public enum Logger_Type
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
    }

}

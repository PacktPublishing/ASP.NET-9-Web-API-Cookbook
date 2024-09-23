using System;
using System.IO;

public static class TestLogger
{
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_log.txt");

    static TestLogger()
    {
        // Clear the log file at the start of each test run
        File.WriteAllText(LogFilePath, string.Empty);
    }

    public static void Log(string message)
    {
        var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}";
        Console.WriteLine(logMessage);
        File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
    }
}

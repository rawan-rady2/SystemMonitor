using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using Timer = System.Timers.Timer;

class SystemMonitor
{
    private static Timer timer;
    private static PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    private static PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
    private static DriveInfo drive = new DriveInfo("C"); // Monitor C: drive
    private static string logFile = "SystemHealthLog.txt";

    static void Main()
    {
        // Warm-up CPU counter to avoid the first reading being 0.00%
        cpuCounter.NextValue();

        timer = new Timer(5000); // Set interval to 5 seconds
        timer.Elapsed += MonitorSystem;
        timer.AutoReset = true;
        timer.Start();

        Console.WriteLine("System monitoring started. Press Enter to exit.");
        Console.ReadLine();
    }

    private static void MonitorSystem(object sender, ElapsedEventArgs e)
    {
        float cpuUsage = cpuCounter.NextValue();
        float availableMemory = ramCounter.NextValue();
        float totalMemory = GetTotalMemory();
        float usedMemory = totalMemory - availableMemory;
        float diskUsage = GetDiskUsage();

        string logMessage = $"[{DateTime.Now}] CPU: {cpuUsage:F2}% | Memory: {usedMemory}/{totalMemory}MB | Disk Usage: {diskUsage:F2}%";

        LogToFile(logMessage);
        Console.WriteLine(logMessage);
    }

    private static float GetTotalMemory()
    {
        return (float)new PerformanceCounter("Memory", "Committed Bytes").RawValue / (1024 * 1024);
    }

    private static float GetDiskUsage()
    {
        float totalSpace = drive.TotalSize / (1024 * 1024 * 1024);
        float freeSpace = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
        return ((totalSpace - freeSpace) / totalSpace) * 100;
    }

    private static void LogToFile(string message)
    {
        using (StreamWriter writer = new StreamWriter(logFile, true))
        {
            writer.WriteLine(message);
        }
    }
}

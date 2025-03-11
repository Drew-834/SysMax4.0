using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SysMax2._1.Models
{
    /// <summary>
    /// Service class to handle assistant interactions (if missing)
    /// </summary>
    public class AssistantService
    {
        // Messages dictionary to store helpful messages for each context
        private readonly Dictionary<string, string> _messages = new Dictionary<string, string>
        {
            { "SystemOverview", "This dashboard shows you an overview of your system's health and performance. Green indicators are good, yellow indicate attention might be needed, and red shows issues that should be addressed." },
            { "CPU", "This page shows detailed information about your CPU (processor) performance, including usage, temperature, and other metrics." },
            { "Memory", "This page shows information about your RAM (memory) usage. Having too little available memory can slow down your computer." },
            { "Storage", "This page shows details about your storage drives, including available space and performance. Low disk space can affect system performance." },
            { "Network", "This page shows information about your network connections, including speed and status." },
            { "Issues", "This page shows detected issues that might affect your system's performance or stability, with options to fix them." },
            { "Settings", "Here you can customize SysMax to suit your preferences, including monitoring settings and notification options." },
            { "Help", "Find tutorials and support information to help you get the most out of SysMax." },
            { "Diagnostics", "Run diagnostics to identify any issues with your system's hardware or software." },
            { "Optimization", "Tools to improve your system's performance by cleaning up unnecessary files and optimizing settings." },
            { "SystemInfo", "View detailed information about your computer's hardware and software configuration." },
            { "Logs", "View system and application logs to troubleshoot issues or monitor system activity." },
            { "HighCPU", "Your CPU usage is very high. This could be because demanding applications are running. Try closing some applications to reduce the load." },
            { "LowMemory", "Your system is running low on memory. Close some applications to free up memory and improve performance." },
            { "LowDiskSpace", "Your disk space is running low. This can affect system performance and prevent updates from installing. Try cleaning up unnecessary files." },
            { "NetworkDisconnected", "Your network connection appears to be down. Check your network settings and connections." }
        };

        public string GetMessage(string key)
        {
            if (_messages.TryGetValue(key, out string message))
            {
                return message;
            }

            return "I'm here to help you understand your system better. What would you like to know?";
        }

        public string GetContextAwareMessage(int cpuUsage, int memoryUsage, int diskSpace, bool isNetworkConnected)
        {
            // Check for issues in priority order (most critical first)
            if (!isNetworkConnected)
            {
                return "Your network connection appears to be down. Check that your Wi-Fi is turned on, or that your network cable is connected properly.";
            }

            if (diskSpace < 10)
            {
                return "Your computer is running very low on disk space. This can slow down your system and prevent updates from installing. Consider removing unused files or programs.";
            }

            if (memoryUsage > 90)
            {
                return "Your memory usage is very high. This can cause your computer to slow down. Try closing some applications you're not using.";
            }

            if (cpuUsage > 90)
            {
                return "Your CPU usage is very high. This might be due to demanding applications running in the background. Check Task Manager to see which programs are using the most CPU.";
            }

            if (memoryUsage > 80)
            {
                return "Your memory usage is high. If your system feels slow, try closing applications you're not currently using.";
            }

            if (diskSpace < 15)
            {
                return "Your disk space is getting low. Consider cleaning up unnecessary files or running Disk Cleanup.";
            }

            // If no issues, return a general message
            return "Your system appears to be running well. All metrics are within normal ranges.";
        }

        public string GetToolExplanation(string toolName)
        {
            switch (toolName.ToLower())
            {
                case "diskcleanup":
                    return "Disk Cleanup helps free up space by removing temporary files and unnecessary system files.";
                case "defragmentation":
                    return "Defragmentation reorganizes files on your hard drive to make them load faster. This is not needed for SSDs.";
                case "systemscan":
                    return "System Scan checks your computer for issues that could affect performance or stability.";
                default:
                    return $"This tool helps optimize your {toolName} performance.";
            }
        }
    }

    /// <summary>
    /// Model class for representing storage drive information in the UI
    /// </summary>
    public class StorageDriveInfo
    {
        public string DriveName { get; set; } = "";
        public string DriveType { get; set; } = "";
        public string UsageText { get; set; } = "";
        public string UsagePercentage { get; set; } = "0%";
        public System.Windows.Media.SolidColorBrush UsageColor { get; set; } = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
    }

    /// <summary>
    /// Model class for representing network interface information in the UI
    /// </summary>
    public class NetworkInterfaceInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string AddressInfo { get; set; } = "";
        public bool IsConnected { get; set; }
    }

    /// <summary>
    /// Class for resolving converter resources in XAML.
    /// Note: Instead of using the built-in InverseBooleanToVisibilityConverter,
    /// we provide a custom implementation.
    /// </summary>
    public class Converters
    {
        private static BooleanToVisibilityConverter _booleanToVisibilityConverter;
        public static BooleanToVisibilityConverter BooleanToVisibilityConverter
        {
            get
            {
                if (_booleanToVisibilityConverter == null)
                {
                    _booleanToVisibilityConverter = new BooleanToVisibilityConverter();
                }
                return _booleanToVisibilityConverter;
            }
        }

        // Custom converter that inverts a boolean before converting to Visibility.
        public class CustomInverseBooleanToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool booleanValue)
                {
                    // Invert: true becomes Collapsed, false becomes Visible.
                    return booleanValue ? Visibility.Collapsed : Visibility.Visible;
                }
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is Visibility visibility)
                {
                    return visibility != Visibility.Visible;
                }
                return false;
            }
        }

        private static CustomInverseBooleanToVisibilityConverter _customInverseBooleanToVisibilityConverter;
        public static CustomInverseBooleanToVisibilityConverter InverseBooleanToVisibilityConverter
        {
            get
            {
                if (_customInverseBooleanToVisibilityConverter == null)
                {
                    _customInverseBooleanToVisibilityConverter = new CustomInverseBooleanToVisibilityConverter();
                }
                return _customInverseBooleanToVisibilityConverter;
            }
        }
    }
}

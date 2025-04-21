using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using SysMax2._1.Models;
using SysMax2._1.Services;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for SystemInfoPage.xaml
    /// </summary>
    public partial class SystemInfoPage : Page
    {
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private readonly EnhancedHardwareMonitorService _hardwareMonitor;
        private DispatcherTimer _updateTimer;
        private MainWindow? mainWindow;

        public class StorageDriveInfo
        {
            public string DriveName { get; set; } = "";
            public string DriveType { get; set; } = "";
            public string UsageText { get; set; } = "";
            public double UsageValue { get; set; } = 0.0; // Value between 0 and 100
        }

        public class NetworkInterfaceInfo
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string AddressInfo { get; set; } = "";
            public bool IsConnected { get; set; }
        }

        public ObservableCollection<StorageDriveInfo> StorageDrives { get; set; } = new ObservableCollection<StorageDriveInfo>();
        public ObservableCollection<NetworkInterfaceInfo> NetworkInterfaces { get; set; } = new ObservableCollection<NetworkInterfaceInfo>();

        public SystemInfoPage()
        {
            InitializeComponent();

            _hardwareMonitor = EnhancedHardwareMonitorService.Instance;
            
            // Assign and check mainWindow
            mainWindow = Window.GetWindow(this) as MainWindow;
             if (mainWindow == null)
             {
                 _loggingService.Log(LogLevel.Error, "Could not find MainWindow reference in SystemInfoPage.");
                 // Consider disabling features relying on mainWindow (status updates, assistant messages)
             }

            StorageListView.ItemsSource = StorageDrives;
            NetworkListView.ItemsSource = NetworkInterfaces;

            if (!_hardwareMonitor.IsMonitoring)
            {
                _hardwareMonitor.StartMonitoring();
            }

            LoadSystemInformation();

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            _loggingService.Log(LogLevel.Info, "Navigated to System Information page");
        }

        private void LoadSystemInformation()
        {
            try
            {
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Loading system information...");
                }

                LoadBasicSystemInfo();

                LoadHardwareInfo();

                LoadStorageInfo();

                LoadNetworkInfo();

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("System information loaded");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading system information: {ex.Message}");

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Error loading system information");
                    mainWindow.ShowAssistantMessage("There was a problem loading your system information. Some data may not be displayed correctly.");
                }
            }
        }

        private void LoadBasicSystemInfo()
        {
            try
            {
                OSValue.Text = Environment.OSVersion.Platform.ToString();

                OSVersionValue.Text = GetWindowsVersionInfo();

                ComputerNameValue.Text = Environment.MachineName;

                UsernameValue.Text = Environment.UserName;

                UpdateUptimeInfo();
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading basic system info: {ex.Message}");

                OSValue.Text = "Information Unavailable";
                OSVersionValue.Text = "Information Unavailable";
                ComputerNameValue.Text = "Information Unavailable";
                UsernameValue.Text = "Information Unavailable";
                UptimeValue.Text = "Information Unavailable";
                LastBootValue.Text = "Information Unavailable";
            }
        }

        private string GetWindowsVersionInfo()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string? productName = key.GetValue("ProductName") as string;
                        string? releaseId = key.GetValue("ReleaseId") as string;
                        string? buildNumber = key.GetValue("CurrentBuildNumber") as string;
                        string? displayVersion = key.GetValue("DisplayVersion") as string;

                        if (!string.IsNullOrEmpty(productName))
                        {
                            return $"{productName} {displayVersion ?? releaseId ?? ""} (Build {buildNumber ?? Environment.OSVersion.Version.Build.ToString()})";
                        }
                    }
                }

                return Environment.OSVersion.VersionString;
            }
            catch
            {
                return Environment.OSVersion.VersionString;
            }
        }

        private void UpdateUptimeInfo()
        {
            try
            {
                long uptimeMs = Environment.TickCount64;
                TimeSpan uptime = TimeSpan.FromMilliseconds(uptimeMs);
                DateTime bootTime = DateTime.Now - uptime;

                UptimeValue.Text = $"{uptime.Days}d {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
                LastBootValue.Text = bootTime.ToString("f");
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting uptime info: {ex.Message}");
                UptimeValue.Text = "Information Unavailable";
                LastBootValue.Text = "Information Unavailable";
            }
        }

        private void LoadHardwareInfo()
        {
            try
            {
                var systemInfo = _hardwareMonitor.GetSystemInformation();

                CPUValue.Text = $"{systemInfo.GetValueOrDefault("ProcessorName", "Unknown")} ({systemInfo.GetValueOrDefault("CPUCores", "Unknown")} cores)";

                // Get Total RAM from the monitoring service
                string totalRamStr = systemInfo.GetValueOrDefault("TotalRAM", "N/A");
                
                // Get Available RAM using WMI
                string availableRamStr = GetAvailableRamWmi();
                
                // Format the RAM display text
                RAMValue.Text = $"Total: {totalRamStr} (Available: {availableRamStr})";
                
                // Cleaned up previous attempts

                GPUValue.Text = systemInfo.TryGetValue("GPUName", out string? gpuName)
                    ? gpuName
                    : "Information Unavailable";

                StorageValue.Text = systemInfo.TryGetValue("StorageSummary", out string? storageSummary)
                    ? storageSummary
                    : "Information Unavailable";
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading hardware info: {ex.Message}");

                CPUValue.Text = "Information Unavailable";
                RAMValue.Text = "Information Unavailable";
                GPUValue.Text = "Information Unavailable";
                StorageValue.Text = "Information Unavailable";
            }
        }

        private string GetAvailableRamWmi()
        {
            try
            {
                ObjectQuery wmiQuery = new ObjectQuery("SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
                ManagementObjectCollection results = searcher.Get();

                foreach (ManagementObject result in results)
                {
                    // FreePhysicalMemory is in KB, convert to GB
                    ulong freeMemoryKB = (ulong)result["FreePhysicalMemory"];
                    double freeMemoryGB = freeMemoryKB / (1024.0 * 1024.0);
                    return $"{freeMemoryGB:F1} GB";
                }
                return "WMI Query Failed"; // Should not happen if query succeeds
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Warning, $"Failed to query WMI for Available RAM: {ex.Message}");
                return "Error";
            }
        }

        private void LoadStorageInfo()
        {
            try
            {
                StorageDrives.Clear();
                DriveInfo[] drives = DriveInfo.GetDrives();

                foreach (DriveInfo drive in drives)
                {
                    if (!drive.IsReady)
                        continue;

                    double usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                    double usagePercentage = (drive.TotalSize > 0) ? (usedSpace / drive.TotalSize * 100.0) : 0.0;

                    StorageDriveInfo driveInfo = new StorageDriveInfo
                    {
                        DriveName = $"{drive.Name.TrimEnd('\\')} ({drive.VolumeLabel})",
                        DriveType = drive.DriveType.ToString(),
                        UsageText = $"{drive.AvailableFreeSpace / (1024.0 * 1024 * 1024):F1} GB free of {drive.TotalSize / (1024.0 * 1024 * 1024):F1} GB",
                        UsageValue = usagePercentage
                    };

                    StorageDrives.Add(driveInfo);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading storage info: {ex.Message}");
            }
        }

        private void LoadNetworkInfo()
        {
            try
            {
                NetworkInterfaces.Clear();

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.Description.ToLower().Contains("tunnel") ||
                        ni.OperationalStatus != OperationalStatus.Up)
                        continue;

                    string addressInfo = "No IP Address";
                    try
                    {
                        IPInterfaceProperties ipProps = ni.GetIPProperties();

                        var ipv4Addresses = ipProps.UnicastAddresses
                            .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(addr => addr.Address.ToString());

                        if (ipv4Addresses.Any())
                        {
                            addressInfo = string.Join(", ", ipv4Addresses);
                        }
                    }
                    catch
                    {
                        addressInfo = $"Status: {ni.OperationalStatus}";
                    }

                    NetworkInterfaceInfo nicInfo = new NetworkInterfaceInfo
                    {
                        Name = ni.Name,
                        Description = ni.Description,
                        AddressInfo = $"{addressInfo}\nSpeed: {GetNetworkSpeed(ni)}",
                        IsConnected = ni.OperationalStatus == OperationalStatus.Up
                    };

                    NetworkInterfaces.Add(nicInfo);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading network info: {ex.Message}");
            }
        }

        private string GetNetworkSpeed(NetworkInterface ni)
        {
            try
            {
                IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();

                long sentBytes = stats.BytesSent;
                long receivedBytes = stats.BytesReceived;

                double sentMbps = sentBytes * 8 / 1_000_000.0;
                double receivedMbps = receivedBytes * 8 / 1_000_000.0;

                if (sentMbps > 0 || receivedMbps > 0)
                {
                    return $"↓{receivedMbps:F2} Mbps ↑{sentMbps:F2} Mbps";
                }

                long linkSpeedMbps = ni.Speed / 1_000_000;
                if (linkSpeedMbps > 0)
                {
                    return $"{linkSpeedMbps} Mbps (link)";
                }

                return "Unknown Speed";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating network speed: {ex.Message}");
                return "Unknown Speed";
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateUptimeInfo();
            UpdateAvailableRamWmi();
        }

        private void UpdateAvailableRamWmi()
        {
            // Get Total RAM from the monitoring service (needed for the label)
            string totalRamStr = _hardwareMonitor.GetSystemInformation().GetValueOrDefault("TotalRAM", "N/A");
            // Get updated Available RAM using WMI
            string availableRamStr = GetAvailableRamWmi();

            // Update the UI
            RAMValue.Text = $"Total: {totalRamStr} (Available: {availableRamStr})";
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _loggingService.Log(LogLevel.Info, "User refreshed system information");

            LoadSystemInformation();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|HTML files (*.html)|*.html|All files (*.*)|*.*",
                    DefaultExt = ".txt",
                    Title = "Export System Information"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();

                    if (extension == ".html")
                    {
                        ExportSystemInfoAsHtml(saveFileDialog.FileName);
                    }
                    else
                    {
                        ExportSystemInfoAsText(saveFileDialog.FileName);
                    }

                    if (mainWindow != null)
                    {
                        mainWindow.UpdateStatus($"System information exported to {Path.GetFileName(saveFileDialog.FileName)}");
                        mainWindow.ShowAssistantMessage($"Successfully exported system information to {Path.GetFileName(saveFileDialog.FileName)}");
                    }

                    _loggingService.Log(LogLevel.Info, $"User exported system information to {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error exporting system information: {ex.Message}");

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Error exporting system information");
                    mainWindow.ShowAssistantMessage($"There was a problem exporting the system information: {ex.Message}");
                }
            }
        }

        private void ExportSystemInfoAsText(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("SYSTEM INFORMATION REPORT");
                writer.WriteLine($"Generated on: {DateTime.Now:F}");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine();

                writer.WriteLine("SYSTEM OVERVIEW");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine($"OS:             {OSValue.Text}");
                writer.WriteLine($"Version:        {OSVersionValue.Text}");
                writer.WriteLine($"Computer Name:  {ComputerNameValue.Text}");
                writer.WriteLine($"Username:       {UsernameValue.Text}");
                writer.WriteLine($"System Uptime:  {UptimeValue.Text}");
                writer.WriteLine($"Last Boot:      {LastBootValue.Text}");
                writer.WriteLine();

                writer.WriteLine("HARDWARE");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine($"Processor:      {CPUValue.Text}");
                writer.WriteLine($"Memory:         {RAMValue.Text}");
                writer.WriteLine($"Graphics:       {GPUValue.Text}");
                writer.WriteLine($"Storage:        {StorageValue.Text}");
                writer.WriteLine();

                writer.WriteLine("STORAGE DRIVES");
                writer.WriteLine(new string('-', 80));
                foreach (var drive in StorageDrives)
                {
                    writer.WriteLine($"{drive.DriveName} ({drive.DriveType})");
                    writer.WriteLine($"    {drive.UsageText}");
                    writer.WriteLine();
                }

                writer.WriteLine("NETWORK INTERFACES");
                writer.WriteLine(new string('-', 80));
                foreach (var iface in NetworkInterfaces)
                {
                    writer.WriteLine($"{iface.Name} - {(iface.IsConnected ? "Connected" : "Disconnected")}");
                    writer.WriteLine($"    {iface.Description}");
                    writer.WriteLine($"    {iface.AddressInfo}");
                    writer.WriteLine();
                }

                writer.WriteLine("CURRENT SYSTEM METRICS");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine($"CPU Usage:      {_hardwareMonitor.CpuUsage:F1}%");
                writer.WriteLine($"CPU Temperature: {_hardwareMonitor.CpuTemperature:F1}°C");
                writer.WriteLine($"Memory Usage:   {_hardwareMonitor.MemoryUsage:F1}%");
                writer.WriteLine($"Disk Usage:     {_hardwareMonitor.DiskUsage:F1}%");
                writer.WriteLine();

                writer.WriteLine(new string('-', 80));
                writer.WriteLine("Generated by SysMax System Health Monitor");
            }
        }

        private void ExportSystemInfoAsHtml(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine("<title>System Information Report</title>");
                writer.WriteLine("<style>");
                writer.WriteLine("body { font-family: Arial, sans-serif; margin: 40px; color: #333; }");
                writer.WriteLine("h1 { color: #3498db; }");
                writer.WriteLine("h2 { color: #2980b9; margin-top: 30px; border-bottom: 1px solid #eee; padding-bottom: 5px; }");
                writer.WriteLine(".timestamp { color: #888; font-style: italic; margin-bottom: 30px; }");
                writer.WriteLine("table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }");
                writer.WriteLine("th, td { text-align: left; padding: 8px; border-bottom: 1px solid #ddd; }");
                writer.WriteLine("th { background-color: #f2f2f2; }");
                writer.WriteLine(".connected { color: #2ecc71; font-weight: bold; }");
                writer.WriteLine(".disconnected { color: #e74c3c; font-weight: bold; }");
                writer.WriteLine(".footer { margin-top: 50px; text-align: center; color: #888; font-size: 12px; }");
                writer.WriteLine("</style>");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");

                writer.WriteLine("<h1>System Information Report</h1>");
                writer.WriteLine($"<p class=\"timestamp\">Generated on: {DateTime.Now:F}</p>");

                writer.WriteLine("<h2>System Overview</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Property</th><th>Value</th></tr>");
                writer.WriteLine($"<tr><td>OS</td><td>{OSValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Version</td><td>{OSVersionValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Computer Name</td><td>{ComputerNameValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Username</td><td>{UsernameValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>System Uptime</td><td>{UptimeValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Last Boot</td><td>{LastBootValue.Text}</td></tr>");
                writer.WriteLine("</table>");

                writer.WriteLine("<h2>Hardware</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Component</th><th>Details</th></tr>");
                writer.WriteLine($"<tr><td>Processor</td><td>{CPUValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Memory</td><td>{RAMValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Graphics</td><td>{GPUValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Storage</td><td>{StorageValue.Text}</td></tr>");
                writer.WriteLine("</table>");

                writer.WriteLine("<h2>Storage Drives</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Drive</th><th>Type</th><th>Usage</th></tr>");
                foreach (var drive in StorageDrives)
                {
                    writer.WriteLine($"<tr><td>{drive.DriveName}</td><td>{drive.DriveType}</td><td>{drive.UsageText}</td></tr>");
                }
                writer.WriteLine("</table>");

                writer.WriteLine("<h2>Network Interfaces</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Name</th><th>Description</th><th>Status</th><th>Address Info</th></tr>");
                foreach (var iface in NetworkInterfaces)
                {
                    writer.WriteLine($"<tr><td>{iface.Name}</td><td>{iface.Description}</td>" +
                                    $"<td class=\"{(iface.IsConnected ? "connected" : "disconnected")}\">{(iface.IsConnected ? "Connected" : "Disconnected")}</td>" +
                                    $"<td>{iface.AddressInfo}</td></tr>");
                }
                writer.WriteLine("</table>");

                writer.WriteLine("<h2>Current System Metrics</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Metric</th><th>Value</th></tr>");
                writer.WriteLine($"<tr><td>CPU Usage</td><td>{_hardwareMonitor.CpuUsage:F1}%</td></tr>");
                writer.WriteLine($"<tr><td>CPU Temperature</td><td>{_hardwareMonitor.CpuTemperature:F1}°C</td></tr>");
                writer.WriteLine($"<tr><td>Memory Usage</td><td>{_hardwareMonitor.MemoryUsage:F1}%</td></tr>");
                writer.WriteLine($"<tr><td>Disk Usage</td><td>{_hardwareMonitor.DiskUsage:F1}%</td></tr>");
                writer.WriteLine("</table>");

                writer.WriteLine("<div class=\"footer\">Generated by SysMax System Health Monitor</div>");

                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer?.Stop();
        }
    }
}
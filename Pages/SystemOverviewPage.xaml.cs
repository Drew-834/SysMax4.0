using SysMax2._1;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Timers;
using System.Management;
using System.Threading;
using System.Net.NetworkInformation;
#nullable enable

namespace SysMax2._1.Pages
{
    public partial class SystemOverviewPage : Page
    {
        private bool isScanning = false;
        private MainWindow mainWindow;
        private System.Timers.Timer refreshTimer;
        private CancellationTokenSource? cts;

        private PerformanceCounter? cpuCounter;
        private PerformanceCounter? ramCounter;

        private NetworkInterface? activeNetworkInterface;
        private long lastBytesReceived = 0;
        private long lastBytesSent = 0;
        private DateTime lastSample = DateTime.Now;

        public SystemOverviewPage()
        {
            InitializeComponent();

            mainWindow = Window.GetWindow(this) as MainWindow;

            PopulateSystemInfo();
            InitializeCounters();

            refreshTimer = new System.Timers.Timer(2000);
            refreshTimer.Elapsed += (s, e) => Dispatcher.Invoke(UpdateMetrics);
            refreshTimer.AutoReset = true;
            refreshTimer.Start();
        }

        private void InitializeCounters()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

                cpuCounter.NextValue();
                ramCounter.NextValue();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing performance counters: {ex.Message}");
            }
        }

        // ✅ Fixed Quick Actions
        private void QuickAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                switch (button.Name)
                {
                    case "CheckUpdatesButton":
                        OpenWindowsUpdate();
                        break;
                    case "CleanupButton":
                        OpenDiskCleanup();
                        break;
                    case "NetworkDiagnosticsButton":
                        RunNetworkDiagnostics();
                        break;
                    case "StartupAppsButton":
                        OpenStartupApps();
                        break;
                    case "SecurityScanButton":
                        RunSecurityScan();
                        break;
                }
            }
        }

        private void OpenWindowsUpdate()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:windowsupdate-action",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening Windows Update: {ex.Message}");
            }
        }

        private void OpenDiskCleanup()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cleanmgr.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening Disk Cleanup: {ex.Message}");
            }
        }

        private void RunNetworkDiagnostics()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "msdt.exe",
                    Arguments = "/id NetworkDiagnosticsNetworkAdapter",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error running Network Diagnostics: {ex.Message}");
            }
        }

        private void OpenStartupApps()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskmgr.exe",
                    Arguments = "/7",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening Startup Apps: {ex.Message}");
            }
        }

        private void RunSecurityScan()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"Start-MpScan -ScanType QuickScan\"",
                    Verb = "runas",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting Security Scan: {ex.Message}");
            }
        }

        // ✅ Fixed Refresh Button
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateSystemInfo();
            Debug.WriteLine("System information refreshed");
        }

        // ✅ Fixed Run Scan Button
        private async void RunScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (isScanning)
                return;

            try
            {
                isScanning = true;
                RunScanButton.Content = "Scanning...";
                RunScanButton.IsEnabled = false;

                cts?.Cancel();
                cts = new CancellationTokenSource();

                await Task.Run(() => PerformSystemScan(cts.Token), cts.Token);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("System scan was canceled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during system scan: {ex.Message}");
            }
            finally
            {
                RunScanButton.Content = "Run System Scan";
                RunScanButton.IsEnabled = true;
                isScanning = false;
            }
        }

        private void PerformSystemScan(CancellationToken token)
        {
            Thread.Sleep(500);

            Dispatcher.Invoke(UpdateMetrics);
        }

        // ✅ Fixed `FixIssue_Click`
        private void FixIssue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string issueType = button.Tag?.ToString() ?? "";

                switch (issueType)
                {
                    case "DiskSpace":
                        OpenDiskCleanup();
                        break;

                    case "WindowsUpdate":
                        OpenWindowsUpdate();
                        break;

                    default:
                        Debug.WriteLine($"No handler for {issueType}");
                        break;
                }
            }
        }

        private void PopulateSystemInfo()
        {
            try
            {
                // Fill in the system information
                OsInfo.Text = Environment.OSVersion.VersionString;
                CpuInfo.Text = GetProcessorInfo();
                RamInfo.Text = GetMemoryInfo();  // Now correctly fetching memory
                GpuInfo.Text = GetGpuInfo();     // Getting GPU info
                StorageInfo.Text = GetStorageInfo();
                NetworkInfo.Text = GetNetworkInfo();
                ComputerNameInfo.Text = Environment.MachineName;

                // Update additional metrics
                UpdateMetrics();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error populating system info: {ex.Message}");
            }
        }

        private string GetProcessorInfo() =>
            Microsoft.Win32.Registry.LocalMachine
                .OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0")?
                .GetValue("ProcessorNameString") as string ?? "Unknown CPU";

        private string GetMemoryInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        long totalMemory = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                        double totalMemoryInGB = totalMemory / 1024.0 / 1024.0 / 1024.0; // Convert bytes to GB
                        return $"{totalMemoryInGB:F1} GB";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting memory info: {ex.Message}");
            }
            return "Unknown Memory";
        }


        // ✅ Fixed `GetGpuInfo()` - Always Returns a Value
        private string GetGpuInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                foreach (var obj in searcher.Get())
                {
                    return obj["Name"]?.ToString() ?? "Unknown GPU";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting GPU info: {ex.Message}");
            }
            return "Unknown GPU";
        }


        private string GetStorageInfo()
        {
            try
            {
                long totalFree = 0;
                long totalSize = 0;

                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        totalFree += drive.AvailableFreeSpace;
                        totalSize += drive.TotalSize;
                    }
                }

                double freeGB = totalFree / 1024.0 / 1024.0 / 1024.0;
                double totalGB = totalSize / 1024.0 / 1024.0 / 1024.0;
                return $"{freeGB:F1} GB free of {totalGB:F1} GB";
            }
            catch
            {
                return "Unknown Storage";
            }
        }
        private string GetNetworkInfo()
        {
            try
            {
                string result = "";
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                         ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        result += $"{ni.Name} ({ni.NetworkInterfaceType})\n";
                    }
                }

                return string.IsNullOrWhiteSpace(result) ? "No active network connection" : result.TrimEnd('\n');
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting network info: {ex.Message}");
                return "Unknown Network Configuration";
            }
        }

        private void UpdateMetrics()
        {
            try
            {
                // ✅ CPU and Memory Usage
                CpuUsageValue.Text = $"{cpuCounter?.NextValue():F1}%";
                MemoryUsageValue.Text = $"{ramCounter?.NextValue():F1}%";

                // ✅ Network Speed (Adapter Link Speed)
                bool isNetworkConnected = NetworkInterface.GetIsNetworkAvailable();
                long networkSpeed = 0;

                if (isNetworkConnected)
                {
                    if (activeNetworkInterface == null || activeNetworkInterface.OperationalStatus != OperationalStatus.Up)
                    {
                        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                        foreach (NetworkInterface ni in interfaces)
                        {
                            if (ni.OperationalStatus == OperationalStatus.Up &&
                                (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                                 ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                            {
                                activeNetworkInterface = ni;
                                networkSpeed = ni.Speed / 1_000_000; // Convert bits to Mbps
                                Debug.WriteLine($"✅ Active network interface set to: {ni.Name}, Speed: {networkSpeed} Mbps");
                                break;
                            }
                        }
                    }
                    else
                    {
                        networkSpeed = activeNetworkInterface.Speed / 1_000_000;
                    }
                }

                // ✅ Update Network Status
                NetworkStatus.Text = isNetworkConnected ? "Connected" : "Disconnected";
                NetworkSpeedValue.Text = isNetworkConnected
                    ? $"{networkSpeed} Mbps"
                    : "0 Mbps";

                // ✅ Update Health Indicator
                if (!isNetworkConnected)
                {
                    NetworkHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                }
                else if (networkSpeed < 10)
                {
                    NetworkHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                }
                else
                {
                    NetworkHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error updating metrics: {ex.Message}");
                NetworkSpeedValue.Text = "Error";
            }
        }


        ~SystemOverviewPage()
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            cpuCounter?.Dispose();
            ramCounter?.Dispose();
        }
    }
}

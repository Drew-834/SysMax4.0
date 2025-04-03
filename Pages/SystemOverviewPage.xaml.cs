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
using SysMax2._1.Services;
#nullable enable

namespace SysMax2._1.Pages
{
    public partial class SystemOverviewPage : Page
    {
        private bool isScanning = false;
        private MainWindow? mainWindow;
        private System.Timers.Timer? refreshTimer;
        private CancellationTokenSource? cts;

        private readonly EnhancedHardwareMonitorService _hardwareMonitor = EnhancedHardwareMonitorService.Instance;

        public SystemOverviewPage()
        {
            InitializeComponent();

            mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null)
            {
                Debug.WriteLine("CRITICAL ERROR: Could not find MainWindow reference in SystemOverviewPage.");
            }

            if (!_hardwareMonitor.IsMonitoring)
            {
                _hardwareMonitor.StartMonitoring();
            }

            PopulateSystemInfo();
            UpdateMetrics();

            refreshTimer = new System.Timers.Timer(2000);
            refreshTimer.Elapsed += OnRefreshTimerElapsed;
            refreshTimer.AutoReset = true;
            refreshTimer.Start();

            _hardwareMonitor.PropertyChanged += HardwareMonitor_PropertyChanged;
            this.Unloaded += Page_Unloaded;
        }

        private void OnRefreshTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(UpdateMetrics);
        }

        private void HardwareMonitor_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_hardwareMonitor.CpuUsage):
                        CpuUsageValue.Text = $"{_hardwareMonitor.CpuUsage:F1}%";
                        UpdateHealthIndicator(CpuHealthIndicator, _hardwareMonitor.CpuUsage, 90, 70);
                        break;
                    case nameof(_hardwareMonitor.MemoryUsage):
                        MemoryUsageValue.Text = $"{_hardwareMonitor.MemoryUsage:F1}%";
                        UpdateHealthIndicator(MemoryHealthIndicator, _hardwareMonitor.MemoryUsage, 90, 75);
                        break;
                    case nameof(_hardwareMonitor.IsNetworkConnected):
                    case nameof(_hardwareMonitor.NetworkDownloadSpeed):
                    case nameof(_hardwareMonitor.NetworkUploadSpeed):
                        UpdateNetworkStatus();
                        break;
                }
            });
        }

        private void UpdateNetworkStatus()
        {
            bool isConnected = _hardwareMonitor.IsNetworkConnected;
            string speedText = "0 Mbps";
            if (isConnected)
            {
                long linkSpeed = 0;
                try
                {
                    NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface ni in interfaces)
                    {
                        if (ni.OperationalStatus == OperationalStatus.Up &&
                            (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                             ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                        {
                            linkSpeed = ni.Speed / 1_000_000;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting link speed: {ex.Message}");
                }
                speedText = $"{linkSpeed} Mbps";
            }
            NetworkStatus.Text = isConnected ? "Connected" : "Disconnected";
            NetworkSpeedValue.Text = speedText;
            UpdateHealthIndicator(NetworkHealthIndicator, isConnected ? 100 : 0, 1, 0);
        }

        private void UpdateHealthIndicator(System.Windows.Shapes.Ellipse indicator, double value, double highThreshold, double mediumThreshold)
        {
            if (indicator == null) return;
            
            SolidColorBrush colorBrush;
            if (value >= highThreshold)
                colorBrush = Application.Current.FindResource("DangerColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Red);
            else if (value >= mediumThreshold)
                colorBrush = Application.Current.FindResource("WarningColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Orange);
            else
                colorBrush = Application.Current.FindResource("SecondaryColor") as SolidColorBrush ?? new SolidColorBrush(Colors.Green);
                
            indicator.Fill = colorBrush;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            _hardwareMonitor.PropertyChanged -= HardwareMonitor_PropertyChanged;
            cts?.Dispose();
        }

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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateSystemInfo();
            Debug.WriteLine("System information refreshed");
        }

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
                var sysInfo = _hardwareMonitor.GetSystemInformation();

                OsInfo.Text = sysInfo.ContainsKey("OSName") ? sysInfo["OSName"] : Environment.OSVersion.VersionString;
                CpuInfo.Text = sysInfo.ContainsKey("ProcessorName") ? sysInfo["ProcessorName"] : "Unknown CPU";
                RamInfo.Text = sysInfo.ContainsKey("TotalRAM") ? sysInfo["TotalRAM"] : "Unknown Memory";
                GpuInfo.Text = sysInfo.ContainsKey("GPUName") ? sysInfo["GPUName"] : "Unknown GPU";
                StorageInfo.Text = GetStorageInfo();
                NetworkInfo.Text = sysInfo.ContainsKey("NetworkAdapter") ? sysInfo["NetworkAdapter"] : "Unknown Network";
                ComputerNameInfo.Text = sysInfo.ContainsKey("ComputerName") ? sysInfo["ComputerName"] : Environment.MachineName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error populating system info: {ex.Message}");
                OsInfo.Text = Environment.OSVersion.VersionString;
                CpuInfo.Text = "Error Loading";
                RamInfo.Text = "Error Loading";
                GpuInfo.Text = "Error Loading";
                StorageInfo.Text = "Error Loading";
                NetworkInfo.Text = "Error Loading";
                ComputerNameInfo.Text = Environment.MachineName;
            }
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

        private void UpdateMetrics()
        {
            try
            {
                CpuUsageValue.Text = $"{_hardwareMonitor.CpuUsage:F1}%";
                MemoryUsageValue.Text = $"{_hardwareMonitor.MemoryUsage:F1}%";
                
                UpdateHealthIndicator(CpuHealthIndicator, _hardwareMonitor.CpuUsage, 90, 70);
                UpdateHealthIndicator(MemoryHealthIndicator, _hardwareMonitor.MemoryUsage, 90, 75);
                
                UpdateNetworkStatus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error updating metrics: {ex.Message}");
                CpuUsageValue.Text = "Err";
                MemoryUsageValue.Text = "Err";
            }
        }
    }
}

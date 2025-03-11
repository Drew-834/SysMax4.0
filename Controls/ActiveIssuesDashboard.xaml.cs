using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SysMax2._1.Models;
using SysMax2._1.Services;

namespace SysMax2._1.Controls
{
    /// <summary>
    /// Active Issues Dashboard Component that can be reused across the application
    /// </summary>
    public partial class ActiveIssuesDashboard : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<IssueInfo> _activeIssues = new ObservableCollection<IssueInfo>();
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private readonly EnhancedHardwareMonitorService _hardwareMonitor;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Event for when an issue fix button is clicked
        public event EventHandler<IssueFixEventArgs>? IssueFixRequested;

        public ObservableCollection<IssueInfo> ActiveIssues
        {
            get => _activeIssues;
            set
            {
                _activeIssues = value;
                OnPropertyChanged(nameof(ActiveIssues));
                OnPropertyChanged(nameof(HasIssues));
            }
        }

        public bool HasIssues => ActiveIssues.Count > 0;

        public ActiveIssuesDashboard()
        {
            InitializeComponent();

            // Set data context to self for binding
            DataContext = this;

            // Get hardware monitor service
            _hardwareMonitor = EnhancedHardwareMonitorService.Instance;

            // Subscribe to hardware events
            SubscribeToHardwareEvents();

            // Check for initial issues
            CheckForInitialIssues();
        }

        private void SubscribeToHardwareEvents()
        {
            _hardwareMonitor.HighCpuUsageDetected += HardwareMonitor_HighCpuUsageDetected;
            _hardwareMonitor.HighTemperatureDetected += HardwareMonitor_HighTemperatureDetected;
            _hardwareMonitor.HighMemoryUsageDetected += HardwareMonitor_HighMemoryUsageDetected;
            _hardwareMonitor.LowDiskSpaceDetected += HardwareMonitor_LowDiskSpaceDetected;
            _hardwareMonitor.NetworkDisconnected += HardwareMonitor_NetworkDisconnected;
        }

        private void CheckForInitialIssues()
        {
            if (_hardwareMonitor.CpuUsage > _hardwareMonitor.HighCpuThreshold)
            {
                AddIssue("HighCPU", "CPU usage is high",
                    $"CPU usage is at {_hardwareMonitor.CpuUsage:F1}%, which may slow down your system.",
                    "Show Details", IssueInfo.Severity.Medium);
            }

            if (_hardwareMonitor.CpuTemperature > _hardwareMonitor.HighTemperatureThreshold)
            {
                AddIssue("HighTemperature", "CPU temperature is high",
                    $"CPU temperature is at {_hardwareMonitor.CpuTemperature:F1}°C, which is above the recommended limit.",
                    "Show Details", IssueInfo.Severity.Medium);
            }

            if (_hardwareMonitor.MemoryUsage > _hardwareMonitor.HighMemoryThreshold)
            {
                AddIssue("HighMemory", "Memory usage is high",
                    $"Memory usage is at {_hardwareMonitor.MemoryUsage:F1}%, which may slow down your system.",
                    "Fix Now", IssueInfo.Severity.Medium);
            }

            if (_hardwareMonitor.AvailableDiskSpace < _hardwareMonitor.LowDiskSpaceThreshold)
            {
                double gbAvailable = _hardwareMonitor.AvailableDiskSpace / (1024.0 * 1024 * 1024);
                AddIssue("DiskSpace", "Disk space is low",
                    $"You have {gbAvailable:F1} GB of disk space remaining, which is below the recommended minimum.",
                    "Fix Now", IssueInfo.Severity.High);
            }

            if (!_hardwareMonitor.IsNetworkConnected)
            {
                AddIssue("NetworkDisconnected", "Network is disconnected",
                    "Your computer is not connected to any network. Check your network settings or Wi-Fi connection.",
                    "Network Settings", IssueInfo.Severity.High);
            }

            // ✅ Check for Windows updates
            CheckForWindowsUpdates();
        }

        private void CheckForWindowsUpdates()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c start ms-settings:windowsupdate-action",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Failed to check for updates: {ex.Message}");
                MessageBox.Show($"Failed to check for updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ FIX: Run a Quick Security Scan using Windows Defender
        private void RunQuickSecurityScan()
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

                MessageBox.Show("Quick Security Scan started.", "Security Scan", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Failed to start security scan: {ex.Message}");
                MessageBox.Show($"Failed to start security scan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Hardware Event Handlers

        private void HardwareMonitor_HighCpuUsageDetected(object? sender, EventArgs e) =>
            AddIssue("HighCPU", "CPU usage is high",
                $"CPU usage is at {_hardwareMonitor.CpuUsage:F1}%, which may slow down your system.",
                "Show Details", IssueInfo.Severity.Medium);

        private void HardwareMonitor_HighTemperatureDetected(object? sender, EventArgs e) =>
            AddIssue("HighTemperature", "CPU temperature is high",
                $"CPU temperature is at {_hardwareMonitor.CpuTemperature:F1}°C, which is above the recommended limit.",
                "Show Details", IssueInfo.Severity.Medium);

        private void HardwareMonitor_HighMemoryUsageDetected(object? sender, EventArgs e) =>
            AddIssue("HighMemory", "Memory usage is high",
                $"Memory usage is at {_hardwareMonitor.MemoryUsage:F1}%, which may slow down your system.",
                "Fix Now", IssueInfo.Severity.Medium);

        private void HardwareMonitor_LowDiskSpaceDetected(object? sender, EventArgs e) =>
            AddIssue("DiskSpace", "Disk space is low",
                $"You have {_hardwareMonitor.AvailableDiskSpace / (1024.0 * 1024 * 1024):F1} GB of disk space remaining.",
                "Fix Now", IssueInfo.Severity.High);

        private void HardwareMonitor_NetworkDisconnected(object? sender, EventArgs e) =>
            AddIssue("NetworkDisconnected", "Network is disconnected",
                "Your computer is not connected to any network. Check your network settings or Wi-Fi connection.",
                "Network Settings", IssueInfo.Severity.High);

        #endregion

        public void FixIssue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is IssueInfo issue)
            {
                _loggingService.Log(LogLevel.Info, $"User requested to fix issue: {issue.FixActionTag}");
                IssueFixRequested?.Invoke(this, new IssueFixEventArgs(issue));
            }
        }

        public void AddIssue(string issueType, string title, string description, string buttonText, IssueInfo.Severity severity)
        {
            if (ActiveIssues.Any(issue => issue.FixActionTag == issueType))
                return;

            var newIssue = new IssueInfo
            {
                Icon = GetIconForIssueType(issueType),
                Text = description,
                FixButtonText = buttonText,
                FixActionTag = issueType,
                IssueSeverity = severity,
                Timestamp = DateTime.Now
            };

            ActiveIssues.Add(newIssue);
            OnPropertyChanged(nameof(HasIssues));
        }

        private string GetIconForIssueType(string issueType) =>
            issueType switch
            {
                "HighCPU" => "⚙️",
                "HighTemperature" => "🌡️",
                "HighMemory" => "📊",
                "DiskSpace" => "💾",
                "NetworkDisconnected" => "🌐",
                "WindowsUpdate" => "🔄",
                _ => "⚠️"
            };

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class IssueFixEventArgs : EventArgs
    {
        public IssueInfo Issue { get; }

        public IssueFixEventArgs(IssueInfo issue) => Issue = issue;
    }
}

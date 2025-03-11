using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SysMax2._1.Services;
using SysMax2._1.Models;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for SystemOverviewBasicPage.xaml
    /// </summary>
    public partial class SystemOverviewBasicPage : Page
    {
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private readonly EnhancedHardwareMonitorService _hardwareMonitor;
        private readonly SystemEventLogService _systemEventLogService;
        private DispatcherTimer _updateTimer;
        private MainWindow? mainWindow;
        private int _healthScore = 85;
        private List<IssueInfo> _activeIssues = new List<IssueInfo>();

        public SystemOverviewBasicPage()
        {
            InitializeComponent();

            // Get services
            _hardwareMonitor = EnhancedHardwareMonitorService.Instance;
            _systemEventLogService = new SystemEventLogService();

            // Find main window
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Start hardware monitoring if not already running
            if (!_hardwareMonitor.IsMonitoring)
            {
                _hardwareMonitor.StartMonitoring();
            }

            // Set up update timer
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // Subscribe to hardware events
            SubscribeToHardwareEvents();

            // Initialize UI with current data
            UpdateUI();

            // Check for issues
            CheckForIssues();

            // Show assistant message
            if (mainWindow != null)
            {
                mainWindow.UpdateStatus("Ready");
                mainWindow.ShowAssistantMessage("Welcome to SysMax! This simplified view shows your system health at a glance. Let me know if you need help with anything.");
            }

            // Log page navigation
            _loggingService.Log(LogLevel.Info, "Navigated to System Overview Basic page");
        }

        private void SubscribeToHardwareEvents()
        {
            // Subscribe to hardware monitor events for issue detection
            _hardwareMonitor.HighCpuUsageDetected += (s, e) => Dispatcher.Invoke(CheckForIssues);
            _hardwareMonitor.HighMemoryUsageDetected += (s, e) => Dispatcher.Invoke(CheckForIssues);
            _hardwareMonitor.LowDiskSpaceDetected += (s, e) => Dispatcher.Invoke(CheckForIssues);
            _hardwareMonitor.NetworkDisconnected += (s, e) => Dispatcher.Invoke(CheckForIssues);
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            try
            {
                // Update hardware metrics from monitor service
                float cpuUsage = _hardwareMonitor.CpuUsage;
                float memoryUsage = _hardwareMonitor.MemoryUsage;
                float diskUsage = _hardwareMonitor.DiskUsage;

                // Update CPU UI
                CpuProgressBar.Value = cpuUsage;
                CpuUsageText.Text = $"{cpuUsage:F0}%";

                if (cpuUsage > 90)
                {
                    CpuStatusText.Text = "Very high usage";
                    CpuStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    CpuProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                }
                else if (cpuUsage > 70)
                {
                    CpuStatusText.Text = "High usage";
                    CpuStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    CpuProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                }
                else
                {
                    CpuStatusText.Text = "Normal usage";
                    CpuStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
                    CpuProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498db"));
                }

                // Update Memory UI
                MemoryProgressBar.Value = memoryUsage;
                MemoryUsageText.Text = $"{memoryUsage:F0}%";

                double availableGB = _hardwareMonitor.AvailableMemory / (1024.0 * 1024 * 1024);
                if (memoryUsage > 90)
                {
                    MemoryStatusText.Text = $"Very high usage ({availableGB:F1} GB free)";
                    MemoryStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    MemoryProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                }
                else if (memoryUsage > 75)
                {
                    MemoryStatusText.Text = $"High usage ({availableGB:F1} GB free)";
                    MemoryStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    MemoryProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                }
                else
                {
                    MemoryStatusText.Text = $"Normal usage ({availableGB:F1} GB free)";
                    MemoryStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
                    MemoryProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                }

                // Update Disk UI
                DiskProgressBar.Value = diskUsage;
                DiskUsageText.Text = $"{diskUsage:F0}%";

                double freeSpaceGB = _hardwareMonitor.AvailableDiskSpace / (1024.0 * 1024 * 1024);
                if (diskUsage > 90)
                {
                    DiskStatusText.Text = $"Very low space ({freeSpaceGB:F1} GB free)";
                    DiskStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    DiskProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                }
                else if (diskUsage > 75)
                {
                    DiskStatusText.Text = $"Low space ({freeSpaceGB:F1} GB free)";
                    DiskStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    DiskProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                }
                else
                {
                    DiskStatusText.Text = $"Sufficient space ({freeSpaceGB:F1} GB free)";
                    DiskStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
                    DiskProgressBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9b59b6"));
                }

                // Update health score based on CPU, memory, disk usage and active issues
                UpdateHealthScore();
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating UI: {ex.Message}");
            }
        }

        private void UpdateHealthScore()
        {
            // Calculate health score based on various factors
            float cpuUsage = _hardwareMonitor.CpuUsage;
            float memoryUsage = _hardwareMonitor.MemoryUsage;
            float diskUsage = _hardwareMonitor.DiskUsage;

            // Base score of 100
            int score = 100;

            // Subtract points for high CPU, memory, disk usage
            if (cpuUsage > 90) score -= 15;
            else if (cpuUsage > 70) score -= 5;

            if (memoryUsage > 90) score -= 15;
            else if (memoryUsage > 75) score -= 5;

            if (diskUsage > 90) score -= 15;
            else if (diskUsage > 75) score -= 5;

            // Subtract points for each active issue
            score -= _activeIssues.Count * 10;

            // Ensure score stays within range
            _healthScore = Math.Max(0, Math.Min(100, score));

            // Update health score display
            HealthScoreText.Text = _healthScore.ToString();

            // Update health status text
            if (_healthScore >= 90)
            {
                HealthStatusText.Text = "Your system is in excellent health";
                HealthRecommendationText.Text = "Keep up the good work! No actions needed.";
                HealthScoreText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }
            else if (_healthScore >= 75)
            {
                HealthStatusText.Text = "Your system is in good health";
                HealthRecommendationText.Text = "Minor optimizations possible but not critical.";
                HealthScoreText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498db"));
            }
            else if (_healthScore >= 50)
            {
                HealthStatusText.Text = "Your system needs attention";
                HealthRecommendationText.Text = "Address the highlighted issues to improve performance.";
                HealthScoreText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                HealthStatusText.Text = "Your system is in poor health";
                HealthRecommendationText.Text = "Critical issues need immediate attention!";
                HealthScoreText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
        }

        private void CheckForIssues()
        {
            _activeIssues.Clear();

            try
            {
                // Check for disk space issues
                if (_hardwareMonitor.DiskUsage > 90)
                {
                    _activeIssues.Add(new IssueInfo
                    {
                        Icon = "💾",
                        Text = "Disk space is very low",
                        FixButtonText = "Fix",
                        FixActionTag = "DiskSpace",
                        IssueSeverity = IssueInfo.Severity.High
                    });
                }
                else if (_hardwareMonitor.DiskUsage > 85 && _hardwareMonitor.AvailableDiskSpace < 15L * 1024 * 1024 * 1024) // < 15GB using L to not overflow
                {
                    _activeIssues.Add(new IssueInfo
                    {
                        Icon = "💾",
                        Text = "Disk space is running low",
                        FixButtonText = "Fix",
                        FixActionTag = "DiskSpace",
                        IssueSeverity = IssueInfo.Severity.Medium
                    });
                }

                // Check for Windows updates
                bool updatesNeeded = CheckForWindowsUpdates();
                if (updatesNeeded)
                {
                    _activeIssues.Add(new IssueInfo
                    {
                        Icon = "🔄",
                        Text = "Windows updates available",
                        FixButtonText = "Update",
                        FixActionTag = "WindowsUpdate",
                        IssueSeverity = IssueInfo.Severity.Medium
                    });
                }

                // Check for very high CPU usage
                if (_hardwareMonitor.CpuUsage > 90)
                {
                    _activeIssues.Add(new IssueInfo
                    {
                        Icon = "⚙️",
                        Text = "CPU usage is very high",
                        FixButtonText = "Check",
                        FixActionTag = "HighCPU",
                        IssueSeverity = IssueInfo.Severity.Medium
                    });
                }

                // Update issue display
                UpdateIssueDisplay();
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error checking for issues: {ex.Message}");
            }
        }

        private bool CheckForWindowsUpdates()
        {
            // In a real application, this would check the Windows Update API
            // For now, we'll simulate by checking registry or just return randomly

            try
            {
                // Check registry for pending updates
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update"))
                {
                    if (key != null)
                    {
                        var needReboot = key.GetValue("RebootRequired");
                        if (needReboot != null && Convert.ToBoolean(needReboot))
                        {
                            return true;
                        }
                    }
                }

                // Simulate update check
                Random random = new Random();
                return random.Next(0, 5) == 0; // 20% chance of updates being available
            }
            catch
            {
                // If there's an error, just return false
                return false;
            }
        }

        private void UpdateIssueDisplay()
        {
            // Clear issue display
            Issue1Border.Visibility = Visibility.Collapsed;
            Issue2Border.Visibility = Visibility.Collapsed;

            if (_activeIssues.Count == 0)
            {
                // No issues
                NoIssuesText.Visibility = Visibility.Visible;
                IssuesList.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Show issues
                NoIssuesText.Visibility = Visibility.Collapsed;
                IssuesList.Visibility = Visibility.Visible;

                // Display the first issue
                if (_activeIssues.Count > 0)
                {
                    Issue1Border.Visibility = Visibility.Visible;
                    Issue1Icon.Text = _activeIssues[0].Icon;
                    Issue1Text.Text = _activeIssues[0].Text;
                    Issue1Button.Content = _activeIssues[0].FixButtonText;
                    Issue1Button.Tag = _activeIssues[0].FixActionTag;
                }

                // Display the second issue if available
                if (_activeIssues.Count > 1)
                {
                    Issue2Border.Visibility = Visibility.Visible;
                    Issue2Icon.Text = _activeIssues[1].Icon;
                    Issue2Text.Text = _activeIssues[1].Text;
                    Issue2Button.Content = _activeIssues[1].FixButtonText;
                    Issue2Button.Tag = _activeIssues[1].FixActionTag;
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Update UI data
            UpdateUI();

            // Check for issues
            CheckForIssues();

            // Show status message
            if (mainWindow != null)
            {
                mainWindow.UpdateStatus("System information refreshed");
            }

            // Log the action
            _loggingService.Log(LogLevel.Info, "User refreshed System Overview");
        }

        private void FixIssue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string issueType = button.Tag?.ToString() ?? "";

                // Look for the MainWindow parent
                MainWindow mainWindow = Window.GetWindow(this) as MainWindow;

                // Navigate to the IssueDetailsPage for the specific issue
                if (mainWindow != null)
                {
                    // Use reflection to access the method if it's not directly accessible
                    var method = mainWindow.GetType().GetMethod("NavigateToPage",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (method != null)
                    {
                        // Create the IssueDetailsPage instance with the specific issue type
                        var issueDetailsPage = new Pages.IssueDetailsPage(issueType);

                        // Set the content frame to show the issue details
                        mainWindow.MainContentFrame.Navigate(issueDetailsPage);

                        // Log the navigation
                        _loggingService.Log(LogLevel.Info, $"Navigated to issue details for {issueType}");
                    }
                    else
                    {
                        // Fallback to direct action if navigation fails
                        FallbackFixIssue(issueType);
                    }
                }
                else
                {
                    // Fallback to direct action if navigation fails
                    FallbackFixIssue(issueType);
                }
            }
        }

        private void FallbackFixIssue(string issueType)
        {
            // In a real application, these would perform actual fixes
            switch (issueType)
            {
                case "DiskSpace":
                    try
                    {
                        Process.Start("cleanmgr.exe");
                        _loggingService.Log(LogLevel.Info, "User initiated fix for disk space issue");
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Log(LogLevel.Error, $"Error launching Disk Cleanup: {ex.Message}");
                    }

                    if (mainWindow != null)
                    {
                        mainWindow.ShowAssistantMessage("I'm launching Disk Cleanup to help free up space. This will remove temporary files and empty your Recycle Bin, which helps your computer run better.");
                    }
                    break;

                case "WindowsUpdate":
                    try
                    {
                        Process.Start("ms-settings:windowsupdate");
                        _loggingService.Log(LogLevel.Info, "User initiated fix for Windows Update issue");
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Log(LogLevel.Error, $"Error launching Windows Update: {ex.Message}");
                    }

                    if (mainWindow != null)
                    {
                        mainWindow.ShowAssistantMessage("Installing Windows updates helps keep your computer secure and working properly. After updates are installed, you might need to restart your computer.");
                    }
                    break;

                case "HighCPU":
                    try
                    {
                        Process.Start("taskmgr.exe");
                        _loggingService.Log(LogLevel.Info, "User initiated fix for high CPU issue");
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Log(LogLevel.Error, $"Error launching Task Manager: {ex.Message}");
                    }

                    if (mainWindow != null)
                    {
                        mainWindow.ShowAssistantMessage("I've opened Task Manager so you can see which programs are using the most CPU. You may want to close any programs you aren't using to reduce CPU load.");
                    }
                    break;
            }
        }

        private void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("ms-settings:windowsupdate")
                {
                    UseShellExecute = true
                });

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Checking for Windows updates...");
                    mainWindow.ShowAssistantMessage("I've opened Windows Update so you can check for and install any available updates.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error launching Windows Update: {ex.Message}");
                MessageBox.Show("Failed to open Windows Update. Please try manually from system settings.");
            }
        }



        private void ScanSystemButton_Click(object sender, RoutedEventArgs e)
        {
            // Log the action
            _loggingService.Log(LogLevel.Info, "User clicked System Scan button");

            // Show scanning animation/feedback
            if (mainWindow != null)
            {
                mainWindow.UpdateStatus("Scanning system...");
                mainWindow.ShowAssistantMessage("I'm scanning your system for issues. This may take a moment...");
            }

            // Simulate a scan by rechecking issues after a delay
            DispatcherTimer scanTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            scanTimer.Tick += (s, e2) =>
            {
                // Stop the timer
                scanTimer.Stop();

                // Check for issues
                CheckForIssues();

                // Update UI
                UpdateUI();

                // Show completion message
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("System scan complete");

                    if (_activeIssues.Count > 0)
                    {
                        mainWindow.ShowAssistantMessage($"Scan complete. I found {_activeIssues.Count} issue(s) that need your attention.");
                    }
                    else
                    {
                        mainWindow.ShowAssistantMessage("Good news! The scan is complete and your system appears to be running well. No issues were detected.");
                    }
                }
            };

            // Start the timer
            scanTimer.Start();
        }

        private void CleanupButton_Click(object sender, RoutedEventArgs e)
        {
            // Log the action
            _loggingService.Log(LogLevel.Info, "User clicked Disk Cleanup button");

            try
            {
                // Launch Disk Cleanup
                Process.Start("cleanmgr.exe");

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Launching Disk Cleanup...");
                    mainWindow.ShowAssistantMessage("I've launched Disk Cleanup to help you free up disk space. This will remove temporary files, empty your Recycle Bin, and potentially clean up other unnecessary files.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error launching Disk Cleanup: {ex.Message}");
            }
        }
    }
}
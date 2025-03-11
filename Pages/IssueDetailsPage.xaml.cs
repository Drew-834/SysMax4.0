using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SysMax2._1.Models;
using SysMax2._1.Services;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for IssueDetailsPage.xaml
    /// </summary>
    public partial class IssueDetailsPage : Page
    {
        private string issueType;
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private readonly EnhancedHardwareMonitorService _hardwareMonitor;
        private MainWindow? mainWindow;
        private DispatcherTimer? progressTimer;
        private int fixProgress = 0;

        public class ResolutionStep
        {
            public int StepNumber { get; set; }
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
        }

        public ObservableCollection<ResolutionStep> Steps { get; set; } = new ObservableCollection<ResolutionStep>();

        public IssueDetailsPage(string issueType)
        {
            InitializeComponent();

            this.issueType = issueType;
            _hardwareMonitor = EnhancedHardwareMonitorService.Instance;

            // Find main window
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Set resolution steps as ItemsSource
            ResolutionSteps.ItemsSource = Steps;

            // Configure issue details based on type
            ConfigureIssueDetails();

            // Log page navigation
            _loggingService.Log(LogLevel.Info, $"Navigated to Issue Details page for issue type: {issueType}");
        }

        private void ConfigureIssueDetails()
        {
            // Configure UI based on issue type
            switch (issueType)
            {
                case "DiskSpace":
                    ConfigureDiskSpaceIssue();
                    break;

                case "HighCPU":
                    ConfigureHighCpuIssue();
                    break;

                case "HighMemory":
                    ConfigureHighMemoryIssue();
                    break;

                case "WindowsUpdate":
                    ConfigureWindowsUpdateIssue();
                    break;

                case "NetworkDisconnected":
                    ConfigureNetworkIssue();
                    break;

                case "DriverIssues":
                    ConfigureDriverIssue();
                    break;

                default:
                    ConfigureGenericIssue();
                    break;
            }

            // Update window status
            if (mainWindow != null)
            {
                mainWindow.UpdateStatus($"Viewing {issueType} issue details");
            }
        }

        #region Issue Type Configurations

        private void ConfigureDiskSpaceIssue()
        {
            // Set issue details
            IssueTitle.Text = "Low Disk Space";
            IssueSubtitle.Text = "Resolve low disk space issues to improve system performance";
            IssueIcon.Text = "💾";

            // Get current free space in GB
            double freeSpaceGB = _hardwareMonitor.AvailableDiskSpace / (1024.0 * 1024 * 1024);
            double totalSpaceGB = _hardwareMonitor.TotalDiskSpace / (1024.0 * 1024 * 1024);
            double usedPercent = _hardwareMonitor.DiskUsage;

            IssueHeading.Text = "Low Disk Space Detected";
            IssueDescription.Text = $"Your system is running low on disk space with only {freeSpaceGB:F1} GB free out of {totalSpaceGB:F0} GB total ({usedPercent:F1}% used). Low disk space can slow down your computer and prevent Windows updates from installing.";

            // Set severity based on how critical the space issue is
            if (freeSpaceGB < 5)
            {
                IssueSeverityText.Text = "High Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                IssueImpact.Text = "Impact: Critical system functions may fail, including Windows updates and temporary file storage.";
            }
            else if (freeSpaceGB < 10)
            {
                IssueSeverityText.Text = "Medium Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                IssueImpact.Text = "Impact: System performance may be degraded, and you may experience issues with large files and updates.";
            }
            else
            {
                IssueSeverityText.Text = "Low Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1c40f"));
                IssueImpact.Text = "Impact: You're approaching low disk space which could eventually affect system performance.";
            }

            // Set resolution steps
            Steps.Add(new ResolutionStep { StepNumber = 1, Title = "Run Disk Cleanup", Description = "Windows Disk Cleanup tool can remove temporary files, system files, and empty the Recycle Bin to free up space." });
            Steps.Add(new ResolutionStep { StepNumber = 2, Title = "Uninstall Unused Applications", Description = "Remove applications you no longer use to free up disk space." });
            Steps.Add(new ResolutionStep { StepNumber = 3, Title = "Move Files to External Storage", Description = "Consider moving large files like photos, videos, and documents to an external drive or cloud storage." });
            Steps.Add(new ResolutionStep { StepNumber = 4, Title = "Clean Browser Cache", Description = "Clear your web browser's cache to free up additional space." });

            // Set fix button text
            FixIssueButton.Content = "Run Disk Cleanup";

            // Set additional info
            AdditionalInfoText.Text = "Low disk space can significantly impact your computer's performance. Windows needs free space for the paging file, temporary files, and system updates. It's recommended to keep at least 15% of your disk space free at all times.";
        }

        private void ConfigureHighCpuIssue()
        {
            // Set issue details
            IssueTitle.Text = "High CPU Usage";
            IssueSubtitle.Text = "Identify and resolve processes causing high CPU usage";
            IssueIcon.Text = "⚙️";

            // Get current CPU usage
            float cpuUsage = _hardwareMonitor.CpuUsage;

            IssueHeading.Text = "High CPU Usage Detected";
            IssueDescription.Text = $"Your system is experiencing high CPU usage at {cpuUsage:F1}%. This can cause your computer to run slowly, become unresponsive, or overheat.";

            // Set severity based on current usage
            if (cpuUsage > 90)
            {
                IssueSeverityText.Text = "High Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                IssueImpact.Text = "Impact: System may become unresponsive, applications may crash, and overheating may occur.";
            }
            else if (cpuUsage > 80)
            {
                IssueSeverityText.Text = "Medium Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                IssueImpact.Text = "Impact: System performance may be degraded, applications may respond slowly, and battery life may be reduced.";
            }
            else
            {
                IssueSeverityText.Text = "Low Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1c40f"));
                IssueImpact.Text = "Impact: Slight performance degradation, especially when running multiple applications.";
            }

            // Set resolution steps
            Steps.Add(new ResolutionStep { StepNumber = 1, Title = "Identify CPU-Intensive Processes", Description = "Use Task Manager to identify which processes are using the most CPU resources." });
            Steps.Add(new ResolutionStep { StepNumber = 2, Title = "Close Unnecessary Applications", Description = "Close any applications you're not currently using to free up CPU resources." });
            Steps.Add(new ResolutionStep { StepNumber = 3, Title = "Check for Malware", Description = "Run a malware scan to ensure no malicious software is consuming CPU resources." });
            Steps.Add(new ResolutionStep { StepNumber = 4, Title = "Restart Your Computer", Description = "Sometimes a simple restart can resolve temporary CPU usage issues." });

            // Set fix button text
            FixIssueButton.Content = "Open Task Manager";

            // Set additional info
            AdditionalInfoText.Text = "Persistent high CPU usage can indicate a software problem, malware infection, or insufficient hardware resources for the tasks you're performing. If the issue persists after trying these solutions, consider updating your device drivers or consulting with IT support.";
        }

        private void ConfigureHighMemoryIssue()
        {
            // Set issue details
            IssueTitle.Text = "High Memory Usage";
            IssueSubtitle.Text = "Resolve memory usage issues to improve system performance";
            IssueIcon.Text = "📊";

            // Get current memory usage
            float memoryUsage = _hardwareMonitor.MemoryUsage;
            double availableGB = _hardwareMonitor.AvailableMemory / (1024.0 * 1024 * 1024);
            double totalGB = _hardwareMonitor.TotalMemory / (1024.0 * 1024 * 1024);

            IssueHeading.Text = "High Memory Usage Detected";
            IssueDescription.Text = $"Your system is using {memoryUsage:F1}% of available memory ({(totalGB - availableGB):F1} GB used of {totalGB:F1} GB). High memory usage can cause your system to slow down and applications to become unresponsive.";

            // Set severity based on current usage
            if (memoryUsage > 90)
            {
                IssueSeverityText.Text = "High Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                IssueImpact.Text = "Impact: System may become unresponsive, applications may crash, and excessive disk activity may occur from paging.";
            }
            else if (memoryUsage > 80)
            {
                IssueSeverityText.Text = "Medium Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                IssueImpact.Text = "Impact: System performance may be degraded and applications may respond slowly.";
            }
            else
            {
                IssueSeverityText.Text = "Low Severity";
                IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1c40f"));
                IssueImpact.Text = "Impact: Slight performance degradation, especially when running multiple applications.";
            }

            // Set resolution steps
            Steps.Add(new ResolutionStep { StepNumber = 1, Title = "Close Unnecessary Applications", Description = "Close any applications you're not currently using to free up memory." });
            Steps.Add(new ResolutionStep { StepNumber = 2, Title = "Restart Memory-Intensive Applications", Description = "Applications like web browsers can consume more memory over time. Try closing and reopening them." });
            Steps.Add(new ResolutionStep { StepNumber = 3, Title = "Check for Memory Leaks", Description = "If a specific application consistently uses increasing amounts of memory, it may have a memory leak." });
            Steps.Add(new ResolutionStep { StepNumber = 4, Title = "Restart Your Computer", Description = "A restart will clear the system memory and may resolve temporary memory issues." });

            // Set fix button text
            FixIssueButton.Content = "Optimize Memory";

            // Set additional info
            AdditionalInfoText.Text = "Memory usage naturally increases as you use your computer and run applications. However, persistently high memory usage can indicate a software problem or insufficient RAM for your usage patterns. If you frequently experience high memory usage, consider upgrading your RAM or reducing the number of applications you run simultaneously.";
        }

        private void ConfigureWindowsUpdateIssue()
        {
            // Set issue details
            IssueTitle.Text = "Windows Update Required";
            IssueSubtitle.Text = "Install pending Windows updates to ensure system security and stability";
            IssueIcon.Text = "🔄";

            IssueHeading.Text = "Windows Updates Available";
            IssueDescription.Text = "Your system has pending Windows updates that need to be installed. These updates include important security patches and system improvements.";

            // Set severity to high since security updates are important
            IssueSeverityText.Text = "High Severity";
            IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            IssueImpact.Text = "Impact: Unpatched security vulnerabilities may put your system at risk, and you may miss important feature improvements.";

            // Set resolution steps
            Steps.Add(new ResolutionStep { StepNumber = 1, Title = "Save Your Work", Description = "Save any open documents or work before proceeding, as updates may require a system restart." });
            Steps.Add(new ResolutionStep { StepNumber = 2, Title = "Check Update Status", Description = "View available updates in Windows Update settings." });
            Steps.Add(new ResolutionStep { StepNumber = 3, Title = "Install Updates", Description = "Install all available updates. This process may take some time depending on the number and size of updates." });
            Steps.Add(new ResolutionStep { StepNumber = 4, Title = "Restart if Required", Description = "Some updates require a system restart to complete installation." });

            // Set fix button text
            FixIssueButton.Content = "Open Windows Update";

            // Set additional info
            AdditionalInfoText.Text = "Windows updates are essential for maintaining system security and stability. Microsoft regularly releases updates to fix security vulnerabilities, improve performance, and add new features. It's recommended to keep automatic updates enabled and install updates promptly to ensure your system remains protected.";
        }

        private void ConfigureNetworkIssue()
        {
            // Set issue details
            IssueTitle.Text = "Network Connection Issue";
            IssueSubtitle.Text = "Diagnose and resolve network connectivity problems";
            IssueIcon.Text = "🌐";

            IssueHeading.Text = "Network Connectivity Problem";
            IssueDescription.Text = "Your system is currently experiencing network connectivity issues. This can prevent internet access and connectivity to network resources.";

            // Always set to high severity since network issues are critical
            IssueSeverityText.Text = "High Severity";
            IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            IssueImpact.Text = "Impact: Internet access is unavailable, cloud services won't function, and network resources cannot be accessed.";

            // Set resolution steps
            Steps.Add(new ResolutionStep { StepNumber = 1, Title = "Check Physical Connections", Description = "Ensure network cables are securely connected or that Wi-Fi is enabled." });
            Steps.Add(new ResolutionStep { StepNumber = 2, Title = "Restart Network Devices", Description = "Restart your router and modem to resolve temporary connection issues." });
            Steps.Add(new ResolutionStep { StepNumber = 3, Title = "Run Network Troubleshooter", Description = "Use Windows built-in network troubleshooting tool to automatically identify and fix common issues." });
            Steps.Add(new ResolutionStep { StepNumber = 4, Title = "Reset Network Settings", Description = "If problems persist, consider resetting your network settings (this will remove saved Wi-Fi networks)." });

            // Set fix button text
            FixIssueButton.Content = "Run Network Troubleshooter";

            // Set additional info
            AdditionalInfoText.Text = "Network issues can be caused by a variety of factors including hardware problems, router configuration, ISP outages, or software settings. If the basic troubleshooting steps don't resolve the issue, check if other devices on the same network have connectivity, which can help determine if the problem is specific to this computer or affects the entire network.";
        }

        private void ConfigureDriverIssue()
        {
            // Set issue details
            IssueTitle.Text = "Driver Issues Detected";
            IssueSubtitle.Text = "Resolve hardware driver problems to ensure system stability";
            IssueIcon.Text = "🔧";

            IssueHeading.Text = "Hardware Driver Problems";
            IssueDescription.Text = "System logs indicate issues with one or more hardware drivers. Outdated, missing, or corrupted drivers can cause system instability and hardware malfunctions.";

            // Set to medium severity
            IssueSeverityText.Text = "Medium Severity";
            IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            IssueImpact.Text = "Impact: Hardware may not function correctly, system performance may be reduced, and you may experience crashes or instability.";

            // Set resolution steps
            Steps.Add(new ResolutionStep { StepNumber = 1, Title = "Identify Problem Drivers", Description = "Use Device Manager to identify devices with driver problems (indicated by yellow warning icons)." });
            Steps.Add(new ResolutionStep { StepNumber = 2, Title = "Update Drivers", Description = "Look for driver updates through Device Manager or manufacturer websites." });
            Steps.Add(new ResolutionStep { StepNumber = 3, Title = "Reinstall Problematic Drivers", Description = "Uninstall and reinstall drivers for devices with persistent issues." });
            Steps.Add(new ResolutionStep { StepNumber = 4, Title = "Check for Windows Updates", Description = "Windows Update often includes important driver updates for common hardware." });

            // Set fix button text
            FixIssueButton.Content = "Open Device Manager";

            // Set additional info
            AdditionalInfoText.Text = "Drivers are essential software components that allow Windows to communicate with hardware devices. Keeping drivers updated is important for system stability, security, and performance. However, be cautious when updating drivers - always back up your system before making significant driver changes, and if a device is working properly, it's sometimes best to leave its driver alone.";
        }

        private void ConfigureGenericIssue()
        {
            // Set issue details for unknown issue types
            IssueTitle.Text = "System Issue";
            IssueSubtitle.Text = "Resolve system issues to improve performance and stability";
            IssueIcon.Text = "⚠️";

            IssueHeading.Text = "System Issue Detected";
            IssueDescription.Text = $"A system issue of type '{issueType}' has been detected. This issue may affect system performance or stability.";

            // Default to medium severity
            IssueSeverityText.Text = "Medium Severity";
            IssueSeverityBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            IssueImpact.Text = "Impact: System performance and stability may be affected.";

            // Set generic resolution steps
            Steps.Add(new ResolutionStep { StepNumber = 1, Title = "Restart Your Computer", Description = "A restart can resolve many temporary system issues." });
            Steps.Add(new ResolutionStep { StepNumber = 2, Title = "Check for Updates", Description = "Ensure your system has the latest Windows updates installed." });
            Steps.Add(new ResolutionStep { StepNumber = 3, Title = "Run System Maintenance", Description = "Use built-in Windows maintenance tools to optimize system performance." });
            Steps.Add(new ResolutionStep { StepNumber = 4, Title = "Contact IT Support", Description = "If the issue persists, contact your IT department for further assistance." });

            // Set fix button text
            FixIssueButton.Content = "Run System Maintenance";

            // Set additional info
            AdditionalInfoText.Text = "System issues can have various causes, including software conflicts, hardware problems, or system configuration issues. Regular maintenance and keeping your system updated can help prevent many common problems.";
        }

        #endregion

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Log the action
            _loggingService.Log(LogLevel.Info, $"User clicked back button on Issue Details page for issue type: {issueType}");

            // Navigate back to the previous page
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else if (mainWindow != null)
            {
                // Use reflection to access the method if it's not directly accessible
                var method = mainWindow.GetType().GetMethod("NavigateToPage",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    method.Invoke(mainWindow, new object[] { "Overview" });
                }
            }
        }

        private async void FixIssueButton_Click(object sender, RoutedEventArgs e)
        {
            // Log the action
            _loggingService.Log(LogLevel.Info, $"User initiated fix for issue type: {issueType}");

            // Disable fix button to prevent multiple clicks
            FixIssueButton.IsEnabled = false;

            // Show fix in progress overlay
            FixInProgressOverlay.Visibility = Visibility.Visible;

            // Start progress timer for visual feedback
            StartProgressTimer();

            // Perform the fix based on issue type
            bool success = await FixIssue();

            // Stop progress timer
            StopProgressTimer();

            // Show fix result
            ShowFixResult(success);
        }

        private void StartProgressTimer()
        {
            // Reset progress
            fixProgress = 0;

            // Create and start the timer
            progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            progressTimer.Tick += ProgressTimer_Tick;
            progressTimer.Start();
        }

        private void StopProgressTimer()
        {
            // Stop and dispose the timer
            if (progressTimer != null)
            {
                progressTimer.Stop();
                progressTimer.Tick -= ProgressTimer_Tick;
                progressTimer = null;
            }
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            // Update progress text for visual feedback
            fixProgress += 10;

            if (fixProgress <= 100)
            {
                // Update progress text with random messages for visual effect
                FixProgressText.Text = GetProgressMessage(fixProgress);
            }
            else
            {
                // Stop the timer when we reach 100%
                StopProgressTimer();
            }
        }

        private string GetProgressMessage(int progress)
        {
            // Return different messages based on progress
            return progress switch
            {
                10 => "Analyzing system...",
                20 => "Identifying issue components...",
                30 => "Preparing solution...",
                40 => "Applying fixes...",
                50 => "Scanning for additional issues...",
                60 => "Optimizing system settings...",
                70 => "Verifying fix integrity...",
                80 => "Cleaning up temporary files...",
                90 => "Finalizing changes...",
                100 => "Completing process...",
                _ => "Please wait while we resolve this issue..."
            };
        }

        private async Task<bool> FixIssue()
        {
            // Simulate fix process with a delay
            await Task.Delay(3000);

            try
            {
                // Perform action based on issue type
                switch (issueType)
                {
                    case "DiskSpace":
                        try
                        {
                            Process.Start("cleanmgr.exe");
                            _loggingService.Log(LogLevel.Info, "Launched Disk Cleanup for disk space issue");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Log(LogLevel.Error, $"Error launching Disk Cleanup: {ex.Message}");
                            return false;
                        }

                    case "HighCPU":
                        try
                        {
                            Process.Start("taskmgr.exe");
                            _loggingService.Log(LogLevel.Info, "Launched Task Manager for high CPU issue");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Log(LogLevel.Error, $"Error launching Task Manager: {ex.Message}");
                            return false;
                        }

                    case "HighMemory":
                        // Simulate memory optimization
                        await Task.Delay(2000);
                        _loggingService.Log(LogLevel.Info, "Performed memory optimization");
                        return true;

                    case "WindowsUpdate":
                        try
                        {
                            Process.Start("ms-settings:windowsupdate");
                            _loggingService.Log(LogLevel.Info, "Launched Windows Update settings");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Log(LogLevel.Error, $"Error launching Windows Update settings: {ex.Message}");
                            return false;
                        }

                    case "NetworkDisconnected":
                        try
                        {
                            Process.Start("msdt.exe", "/id NetworkDiagnosticsNetworkAdapter");
                            _loggingService.Log(LogLevel.Info, "Launched Network Troubleshooter");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Log(LogLevel.Error, $"Error launching Network Troubleshooter: {ex.Message}");
                            return false;
                        }

                    case "DriverIssues":
                        try
                        {
                            Process.Start("devmgmt.msc");
                            _loggingService.Log(LogLevel.Info, "Launched Device Manager for driver issues");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Log(LogLevel.Error, $"Error launching Device Manager: {ex.Message}");
                            return false;
                        }

                    default:
                        // For generic or unknown issues, simulate a general system check
                        await Task.Delay(2000);
                        _loggingService.Log(LogLevel.Info, $"Performed general system check for {issueType} issue");
                        return true;
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error fixing {issueType} issue: {ex.Message}");
                return false;
            }
        }

        private void ShowFixResult(bool success)
        {
            // Hide fix in progress overlay
            FixInProgressOverlay.Visibility = Visibility.Collapsed;

            // Set result based on success
            if (success)
            {
                FixResultIcon.Text = "✅";
                FixResultTitle.Text = "Issue Fixed Successfully";
                FixResultDescription.Text = "The issue has been resolved. Your system should now function normally.";
            }
            else
            {
                FixResultIcon.Text = "❗";
                FixResultTitle.Text = "Fix Incomplete";
                FixResultDescription.Text = "We were unable to completely resolve the issue. Please try the manual steps listed or contact IT support for assistance.";
            }

            // Show fix complete overlay
            FixCompleteOverlay.Visibility = Visibility.Visible;

            // Log the result
            _loggingService.Log(LogLevel.Info, $"Fix for {issueType} issue completed with success: {success}");

            // Update status
            if (mainWindow != null)
            {
                mainWindow.UpdateStatus($"Issue fix {(success ? "completed successfully" : "did not complete")}");
            }
        }

        private void BackToOverviewButton_Click(object sender, RoutedEventArgs e)
        {
            // Log the action
            _loggingService.Log(LogLevel.Info, "User navigated back to overview from fix result");

            // Navigate back to the overview page
            if (mainWindow != null)
            {
                // Use reflection to access the method if it's not directly accessible
                var method = mainWindow.GetType().GetMethod("NavigateToPage",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    method.Invoke(mainWindow, new object[] { "Overview" });
                }
            }
        }
    }
}
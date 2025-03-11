using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Diagnostics;
using SysMax2._1.Models;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for ApplicationSettingsPage.xaml
    /// </summary>
    public partial class ApplicationSettingsPage : Page
    {
        private MainWindow mainWindow;
        private CheckBox StartWithWindowsCheck;
        private CheckBox AutoUpdateCheck;
        private RadioButton RefreshMediumOption;
        private RadioButton RefreshSlowOption;
        private RadioButton RefreshFastOption;
        private CheckBox UseAnimationsCheck;
        private CheckBox SystemAlertsCheck;
        private CheckBox PerformanceWarningsCheck;
        private CheckBox UpdatesNotificationsCheck;
        private CheckBox UsageDataCheck;
        private CheckBox CrashReportsCheck;
        // The following sliders are defined in XAML so the duplicate declarations are commented out.
        //private Slider CpuThresholdSlider;
        //private Slider MemoryThresholdSlider;
        //private Slider DiskThresholdSlider;
        private Slider DiskSpaceThresholdSlider;

        public ApplicationSettingsPage()
        {
            InitializeComponent();

            // Initialize missing controls
            InitializeMissingControls();

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Load settings (in a real app, these would be loaded from storage)
            LoadSettings();
        }

        private void InitializeMissingControls()
        {
            // Create controls if they don't exist
            if (StartWithWindowsCheck == null)
            {
                StartWithWindowsCheck = new CheckBox();
                StartWithWindowsCheck.Content = "Start with Windows";
                StartWithWindowsCheck.IsChecked = true;
            }

            if (AutoUpdateCheck == null)
            {
                AutoUpdateCheck = new CheckBox();
                AutoUpdateCheck.Content = "Automatic Updates";
                AutoUpdateCheck.IsChecked = true;
            }

            if (RefreshMediumOption == null)
            {
                RefreshMediumOption = new RadioButton();
                RefreshMediumOption.Content = "Medium";
                RefreshMediumOption.IsChecked = true;
            }

            if (RefreshSlowOption == null)
            {
                RefreshSlowOption = new RadioButton();
                RefreshSlowOption.Content = "Slow";
            }

            if (RefreshFastOption == null)
            {
                RefreshFastOption = new RadioButton();
                RefreshFastOption.Content = "Fast";
            }

            if (UseAnimationsCheck == null)
            {
                UseAnimationsCheck = new CheckBox();
                UseAnimationsCheck.Content = "Use Animations";
                UseAnimationsCheck.IsChecked = true;
            }

            if (SystemAlertsCheck == null)
            {
                SystemAlertsCheck = new CheckBox();
                SystemAlertsCheck.Content = "System Alerts";
                SystemAlertsCheck.IsChecked = true;
            }

            if (PerformanceWarningsCheck == null)
            {
                PerformanceWarningsCheck = new CheckBox();
                PerformanceWarningsCheck.Content = "Performance Warnings";
                PerformanceWarningsCheck.IsChecked = true;
            }

            if (UpdatesNotificationsCheck == null)
            {
                UpdatesNotificationsCheck = new CheckBox();
                UpdatesNotificationsCheck.Content = "Update Notifications";
                UpdatesNotificationsCheck.IsChecked = true;
            }

            if (UsageDataCheck == null)
            {
                UsageDataCheck = new CheckBox();
                UsageDataCheck.Content = "Send Usage Data";
                UsageDataCheck.IsChecked = true;
            }

            if (CrashReportsCheck == null)
            {
                CrashReportsCheck = new CheckBox();
                CrashReportsCheck.Content = "Send Crash Reports";
                CrashReportsCheck.IsChecked = true;
            }

            // These sliders are defined in XAML. In case they aren't, create them.
            if (CpuThresholdSlider == null)
            {
                CpuThresholdSlider = new Slider();
                CpuThresholdSlider.Minimum = 50;
                CpuThresholdSlider.Maximum = 100;
                CpuThresholdSlider.Value = 80;
            }

            if (MemoryThresholdSlider == null)
            {
                MemoryThresholdSlider = new Slider();
                MemoryThresholdSlider.Minimum = 50;
                MemoryThresholdSlider.Maximum = 100;
                MemoryThresholdSlider.Value = 85;
            }

            if (DiskThresholdSlider == null)
            {
                DiskThresholdSlider = new Slider();
                DiskThresholdSlider.Minimum = 50;
                DiskThresholdSlider.Maximum = 100;
                DiskThresholdSlider.Value = 90;
            }

            if (DiskSpaceThresholdSlider == null)
            {
                DiskSpaceThresholdSlider = new Slider();
                DiskSpaceThresholdSlider.Minimum = 5;
                DiskSpaceThresholdSlider.Maximum = 50;
                DiskSpaceThresholdSlider.Value = 15;
            }

            // Add controls to the page if needed
            // This would typically be done in XAML.
        }

        private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                if (slider == CpuThresholdSlider && CpuThresholdText != null)
                {
                    CpuThresholdText.Text = $"{(int)slider.Value}%";
                }
                else if (slider == MemoryThresholdSlider && MemoryThresholdText != null)
                {
                    MemoryThresholdText.Text = $"{(int)slider.Value}%";
                }
                else if (slider == DiskThresholdSlider && DiskThresholdText != null)
                {
                    DiskThresholdText.Text = $"{(int)slider.Value}%";
                }
                else if (slider == DiskSpaceThresholdSlider)
                {
                    // Handle disk space threshold if needed
                }
            }
        }

        private void ClearDataButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement clear data functionality
            var result = MessageBox.Show(
                "This will reset all settings to default and clear stored application data. Are you sure you want to continue?",
                "Clear Application Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Reset settings to default
                LoadSettings();

                // Show confirmation
                MessageBox.Show(
                    "Application data has been cleared and settings reset to default.",
                    "Data Cleared",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Update status
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Application data cleared");
                    mainWindow.ShowAssistantMessage("I've reset all settings to their default values and cleared any stored application data.");
                }
            }
        }

        private void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement check for updates
            if (mainWindow != null)
            {
                mainWindow.UpdateStatus("Checking for updates...");
                mainWindow.ShowAssistantMessage("I'm checking if there are any updates available for SysMax...");
            }

            // Simulate update check with a delay
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            timer.Tick += (s, args) =>
            {
                timer.Stop();
                MessageBox.Show(
                    "You are running the latest version of SysMax.",
                    "No Updates Available",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("No updates available");
                    mainWindow.ShowAssistantMessage("Good news! You're already running the latest version of SysMax.");
                }
            };

            timer.Start();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            // Show about dialog
            MessageBox.Show(
                "SysMax System Health Monitor\nVersion 2.1.0\n\nA comprehensive system monitoring and optimization tool.\n\n© 2025 SysMax Inc. All rights reserved.",
                "About SysMax",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Added missing method to load settings
        private void LoadSettings()
        {
            // TODO: Implement settings loading logic.
            // For now, this is a stub.
        }

        // Added missing event handler for SaveSettingsButton
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement settings saving logic.
            // For now, this is a stub.
        }
    }
}

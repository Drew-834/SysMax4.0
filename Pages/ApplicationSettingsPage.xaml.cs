using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using SysMax2._1.Models;
using SysMax2._1.Services;
using System.IO;
using System.Text.Json;
using System.Reflection; // Needed for Assembly

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for ApplicationSettingsPage.xaml
    /// </summary>
    public partial class ApplicationSettingsPage : Page
    {
        private MainWindow? mainWindow;
        private readonly SettingsService _settingsService;
        private AppSettings _currentSettings; // Initialized in constructor

        public ApplicationSettingsPage()
        {
            InitializeComponent();

            // Use the singleton instance and load settings immediately
            _settingsService = SettingsService.Instance;
            _currentSettings = _settingsService.LoadSettings(); // Initialize _currentSettings here

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Apply loaded settings to the UI
            ApplySettingsToUI();
        }

        private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                // Use slider's Name property to identify which slider changed
                if (slider.Name == "CpuThresholdSlider" && CpuThresholdText != null)
                {
                    CpuThresholdText.Text = $"{(int)slider.Value}%";
                }
                else if (slider.Name == "MemoryThresholdSlider" && MemoryThresholdText != null)
                {
                    MemoryThresholdText.Text = $"{(int)slider.Value}%";
                }
                else if (slider.Name == "DiskThresholdSlider" && DiskThresholdText != null)
                {
                    DiskThresholdText.Text = $"{(int)slider.Value}%";
                }
            }
        }

        private void ClearDataButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will reset all settings to default and clear stored application data. Are you sure you want to continue?",
                "Clear Application Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _settingsService.ResetToDefaults();

                    // Reload settings and apply to UI
                    _currentSettings = _settingsService.LoadSettings();
                    ApplySettingsToUI();

                    MessageBox.Show(
                        "Application data has been cleared and settings reset to default.",
                        "Data Cleared",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    if (mainWindow != null)
                    {
                        mainWindow.UpdateStatus("Application data cleared and settings reset");
                        mainWindow.ShowAssistantMessage("I've reset all settings to their default values and cleared the settings file.");
                    }
                }
                catch (Exception ex)
                {
                     MessageBox.Show($"Error clearing data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                     if (mainWindow != null)
                     {
                         mainWindow.UpdateStatus("Error clearing application data");
                         mainWindow.ShowAssistantMessage($"I encountered an error trying to clear the application data: {ex.Message}");
                     }
                }
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            // Get version dynamically from assembly info
            string version = "N/A";
            try
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                version = assembly.GetName().Version?.ToString() ?? "N/A";
                // Remove trailing .0 if present (e.g., 1.0.0.0 -> 1.0.0)
                if (version.EndsWith(".0")) version = version.Substring(0, version.Length - 2);
            }
            catch (Exception ex)
            {
                 // _loggingService.Log(LogLevel.Warning, $"Could not retrieve assembly version: {ex.Message}"); // Logging removed as requested
            }
            
            // Get copyright dynamically
            string copyright = "© 2025 SysMax Inc. All rights reserved."; // Default
             try
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var copyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
                if (copyrightAttribute != null)
                {
                    copyright = copyrightAttribute.Copyright;
                }
            }
            catch (Exception ex)
            {
                 // _loggingService.Log(LogLevel.Warning, $"Could not retrieve assembly copyright: {ex.Message}"); // Logging removed as requested
            }

            MessageBox.Show(
                $"SysMax System Health Monitor\nVersion {version}\n\nA comprehensive system monitoring and optimization tool.\n\n{copyright}",
                "About SysMax",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ApplySettingsToUI()
        {
            // _currentSettings is already loaded

            // Apply settings to UI elements
            try
            {
                // General
                StartWithWindowsCheckbox.IsChecked = _currentSettings.StartWithWindows;
                RunInBackgroundCheckbox.IsChecked = _currentSettings.RunInBackground;
                DefaultUserModeComboBox.SelectedIndex = _currentSettings.GetUserModeIndex();
                // LanguageComboBox.SelectedIndex = 0; // Removed Language
                // ThemeComboBox.SelectedIndex = _currentSettings.GetThemeIndex(); // Removed Theme

                // Notifications
                EnableNotificationsCheckbox.IsChecked = _currentSettings.EnableNotifications;
                CriticalIssuesOnlyCheckbox.IsChecked = _currentSettings.CriticalIssuesOnly;
                NotificationSoundCheckbox.IsChecked = _currentSettings.NotificationSound;

                // Monitoring
                UpdateFrequencyComboBox.SelectedIndex = _currentSettings.GetUpdateFrequencyIndex();
                CpuThresholdSlider.Value = _currentSettings.CpuAlertThreshold;
                MemoryThresholdSlider.Value = _currentSettings.MemoryAlertThreshold;
                DiskThresholdSlider.Value = _currentSettings.DiskAlertThreshold;
                // Manually update TextBlocks after setting Slider values
                CpuThresholdText.Text = $"{_currentSettings.CpuAlertThreshold}%";
                MemoryThresholdText.Text = $"{_currentSettings.MemoryAlertThreshold}%";
                DiskThresholdText.Text = $"{_currentSettings.DiskAlertThreshold}%";

                // Advanced
                EnableLoggingCheckbox.IsChecked = _currentSettings.EnableLogging;
                // AutoUpdateCheckbox.IsChecked = _currentSettings.AutoUpdateCheck; // Removed Auto Update
            }
            catch (Exception ex)
            {
                // Log error or show message if UI update fails
                MessageBox.Show($"Error applying settings to UI: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                 if (mainWindow != null) mainWindow.UpdateStatus("Error loading settings UI");
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Create AppSettings object from UI
            var settingsToSave = new AppSettings
            {
                // General
                StartWithWindows = StartWithWindowsCheckbox.IsChecked ?? false,
                RunInBackground = RunInBackgroundCheckbox.IsChecked ?? false,
                // DefaultUserMode = ((ComboBoxItem)DefaultUserModeComboBox.SelectedItem).Content.ToString(), // Correct way to get selected string
                // Language = ((ComboBoxItem)LanguageComboBox.SelectedItem).Content.ToString(), // Correct way
                // Theme = ((ComboBoxItem)ThemeComboBox.SelectedItem).Content.ToString(), // Correct way

                // Notifications
                EnableNotifications = EnableNotificationsCheckbox.IsChecked ?? false,
                CriticalIssuesOnly = CriticalIssuesOnlyCheckbox.IsChecked ?? false,
                NotificationSound = NotificationSoundCheckbox.IsChecked ?? false,

                // Monitoring
                // UpdateFrequencySeconds = GetFrequencyFromIndex(UpdateFrequencyComboBox.SelectedIndex),
                CpuAlertThreshold = (int)CpuThresholdSlider.Value,
                MemoryAlertThreshold = (int)MemoryThresholdSlider.Value,
                DiskAlertThreshold = (int)DiskThresholdSlider.Value,

                // Advanced
                EnableLogging = EnableLoggingCheckbox.IsChecked ?? false,
                // AutoUpdateCheck = AutoUpdateCheckbox.IsChecked ?? false // Removed Auto Update
            };
            
            // Use helper methods to set string/int values from ComboBox indices
            settingsToSave.SetUserModeFromIndex(DefaultUserModeComboBox.SelectedIndex);
            // settingsToSave.SetLanguageFromIndex(LanguageComboBox.SelectedIndex); // Removed Language
            // settingsToSave.SetThemeFromIndex(ThemeComboBox.SelectedIndex); // Removed Theme
            settingsToSave.SetUpdateFrequencyFromIndex(UpdateFrequencyComboBox.SelectedIndex);


            // Save using the service
            try
            {
                _settingsService.SaveSettings(settingsToSave);
                _currentSettings = settingsToSave; // Update the in-memory settings

                MessageBox.Show("Settings saved successfully.", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Settings saved");
                    mainWindow.ShowAssistantMessage("Your application settings have been saved.");
                }

                // TODO: Apply settings that require immediate action (e.g., theme change, update frequency change) // Also deferred for simplification
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Error saving settings");
                    mainWindow.ShowAssistantMessage($"I encountered an error trying to save your settings: {ex.Message}");
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using SysMax2._1.Services;
using SysMax2._1.Models;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for LogViewerPage.xaml
    /// </summary>
    public partial class LogViewerPage : Page
    {
        private readonly LoggingService _loggingService;
        private readonly SystemEventLogService _systemEventLogService;
        private ObservableCollection<LogEntry> _logs;
        private ICollectionView _logsView;
        private LogEntry? _selectedLog;
        private MainWindow? mainWindow;

        public LogViewerPage()
        {
            InitializeComponent();

            // Get services
            _loggingService = LoggingService.Instance;
            _systemEventLogService = new SystemEventLogService();

            // Find main window
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Initialize collections
            _logs = new ObservableCollection<LogEntry>();
            LogDataGrid.ItemsSource = _logs;

            // Get collection view for filtering
            _logsView = CollectionViewSource.GetDefaultView(_logs);
            _logsView.Filter = LogFilter;

            // Load logs
            LoadLogs();

            // Log page navigation
            _loggingService.Log(LogLevel.Info, "Navigated to Log Viewer page");
        }

        private void LoadLogs()
        {
            try
            {
                // Show loading status
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Loading logs...");
                }

                _logs.Clear();

                // Load application logs
                var appLogs = _loggingService.GetLogEntries();
                foreach (var log in appLogs)
                {
                    _logs.Add(log);
                }

                // Load system event logs
                var systemLogs = _systemEventLogService.GetRecentSystemEvents(100);
                foreach (var log in systemLogs)
                {
                    _logs.Add(log);
                }

                // Sort by timestamp descending (newest first)
                _logsView.SortDescriptions.Clear();
                _logsView.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Descending));

                // Update status
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus($"Loaded {_logs.Count} logs");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading logs: {ex.Message}");

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Error loading logs");
                    mainWindow.ShowAssistantMessage($"There was a problem loading the system logs: {ex.Message}");
                }
            }
        }

        private bool LogFilter(object item)
        {
            if (item is not LogEntry log)
                return false;

            // Filter by log type
            if (LogTypeFilter.SelectedIndex > 0)
            {
                string selectedLogType = ((ComboBoxItem)LogTypeFilter.SelectedItem).Content.ToString() ?? "";

                switch (selectedLogType)
                {
                    case "System Events":
                        if (log.Source != "System" && log.Source != "Windows" && !log.Source.Contains("Event"))
                            return false;
                        break;
                    case "Application Logs":
                        if (log.Source != "SysMax" && log.Source != "Application")
                            return false;
                        break;
                    case "Security Logs":
                        if (!log.Source.Contains("Security") && !log.Source.Contains("Audit"))
                            return false;
                        break;
                    case "Hardware Logs":
                        if (!log.Source.Contains("Hardware") && !log.Source.Contains("Device"))
                            return false;
                        break;
                }
            }

            // Filter by severity
            if (SeverityFilter.SelectedIndex > 0)
            {
                string selectedSeverity = ((ComboBoxItem)SeverityFilter.SelectedItem).Content.ToString() ?? "";

                if (!log.Level.ToString().Equals(selectedSeverity, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();

                return log.Message.ToLower().Contains(searchText) ||
                       log.Source.ToLower().Contains(searchText);
            }

            return true;
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            // Add null check to prevent NullReferenceException
            if (_logsView == null)
            {
                // Re-initialize the collection view if it's null
                if (_logs != null)
                {
                    _logsView = CollectionViewSource.GetDefaultView(_logs);
                    _logsView.Filter = LogFilter;
                }
                else
                {
                    // If logs collection itself is null, we can't proceed
                    return;
                }
            }

            _logsView.Refresh();

            int filteredCount = _logsView.Cast<object>().Count();
            if (mainWindow != null)
            {
                mainWindow.UpdateStatus($"Showing {filteredCount} of {_logs.Count} logs");
            }
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            LogTypeFilter.SelectedIndex = 0;
            SeverityFilter.SelectedIndex = 0;
            SearchTextBox.Clear();

            ApplyFilters();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
            ApplyFilters();

            // Log the action
            _loggingService.Log(LogLevel.Info, "User refreshed logs in the Log Viewer");
        }

        private void LogDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LogDataGrid.SelectedItem is LogEntry selectedLog)
            {
                _selectedLog = selectedLog;

                // Show log details in the status bar
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus($"Selected log: {selectedLog.Timestamp} - {selectedLog.Source}");
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = ".csv",
                    Title = "Export Logs"
                };

                // Show save file dialog
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Get the current filtered logs
                    var filteredLogs = _logsView.Cast<LogEntry>().ToList();

                    // Export logs to file
                    ExportLogs(filteredLogs, saveFileDialog.FileName);

                    // Show success message
                    if (mainWindow != null)
                    {
                        mainWindow.UpdateStatus($"Exported {filteredLogs.Count} logs to {Path.GetFileName(saveFileDialog.FileName)}");
                        mainWindow.ShowAssistantMessage($"Successfully exported {filteredLogs.Count} logs to {Path.GetFileName(saveFileDialog.FileName)}");
                    }

                    // Log the action
                    _loggingService.Log(LogLevel.Info, $"User exported {filteredLogs.Count} logs to {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error exporting logs: {ex.Message}");

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Error exporting logs");
                    mainWindow.ShowAssistantMessage($"There was a problem exporting the logs: {ex.Message}");
                }
            }
        }

        private void ExportLogs(List<LogEntry> logs, string filePath)
        {
            // Determine export format based on file extension
            string extension = Path.GetExtension(filePath).ToLower();

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write header
                if (extension == ".csv")
                {
                    writer.WriteLine("Timestamp,Level,Source,Message");
                }
                else
                {
                    writer.WriteLine("Timestamp\tLevel\tSource\tMessage");
                }

                // Write log entries
                foreach (var log in logs)
                {
                    if (extension == ".csv")
                    {
                        // CSV format with proper escaping
                        writer.WriteLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},\"{log.Level}\",\"{EscapeCsvField(log.Source)}\",\"{EscapeCsvField(log.Message)}\"");
                    }
                    else
                    {
                        // Tab-delimited text format
                        writer.WriteLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\t{log.Level}\t{log.Source}\t{log.Message}");
                    }
                }
            }
        }

        private string EscapeCsvField(string field)
        {
            // Replace double quotes with two double quotes to escape them in CSV
            return field.Replace("\"", "\"\"");
        }
    }
}
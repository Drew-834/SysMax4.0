using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using SysMax2._1.Models;

namespace SysMax2._1.Services
{
    /// <summary>
    /// Service for accessing and analyzing Windows System Event Logs
    /// </summary>
    public class SystemEventLogService
    {
        private readonly LoggingService _loggingService = LoggingService.Instance;

        /// <summary>
        /// Gets recent system events from Windows Event Log
        /// </summary>
        /// <param name="maxEntries">Maximum number of entries to retrieve</param>
        /// <returns>List of log entries</returns>
        public List<LogEntry> GetRecentSystemEvents(int maxEntries = 100)
        {
            List<LogEntry> entries = new List<LogEntry>();

            try
            {
                // Query the Application log first
                entries.AddRange(GetEventsFromLog("Application", maxEntries / 3));

                // Query the System log
                entries.AddRange(GetEventsFromLog("System", maxEntries / 3));

                // Query the Security log if we have permission
                try
                {
                    entries.AddRange(GetEventsFromLog("Security", maxEntries / 3));
                }
                catch (UnauthorizedAccessException)
                {
                    // This is expected if the user doesn't have permission to read the Security log
                    _loggingService.Log(LogLevel.Warning, "Access denied when reading Security event log");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error reading system event logs: {ex.Message}");

                // If we can't read the Windows Event Log, create a fallback entry to explain the error
                entries.Add(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = LogLevel.Error,
                    Source = "SysMax",
                    Message = $"Could not access Windows Event Log: {ex.Message}"
                });
            }

            return entries;
        }

        /// <summary>
        /// Gets events from a specific event log
        /// </summary>
        /// <param name="logName">Name of the log (e.g., "System", "Application")</param>
        /// <param name="maxEntries">Maximum number of entries to retrieve</param>
        /// <returns>List of log entries</returns>
        private List<LogEntry> GetEventsFromLog(string logName, int maxEntries)
        {
            List<LogEntry> entries = new List<LogEntry>();

            try
            {
                // Create query to get recent events from the specified log
                string queryString = $"*[System/TimeCreated[timediff(@SystemTime) <= 604800000]]";
                EventLogQuery query = new EventLogQuery(logName, PathType.LogName, queryString);

                using (EventLogReader reader = new EventLogReader(query))
                {
                    int count = 0;
                    EventRecord record;

                    // Read events until we reach the maximum or run out of events
                    while ((record = reader.ReadEvent()) != null && count < maxEntries)
                    {
                        using (record)
                        {
                            // Map Windows event level to our log level
                            LogLevel level = MapEventLevelToLogLevel(record.Level ?? 0);

                            // Create log entry
                            LogEntry entry = new LogEntry
                            {
                                Timestamp = record.TimeCreated ?? DateTime.Now,
                                Level = level,
                                Source = $"{logName}/{record.ProviderName ?? "Unknown"}",
                                Message = GetEventMessage(record)
                            };

                            entries.Add(entry);
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error reading from {logName} event log: {ex.Message}");

                // Add a log entry about the failure to read this specific log
                entries.Add(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = LogLevel.Warning,
                    Source = "SysMax",
                    Message = $"Could not read from {logName} event log: {ex.Message}"
                });
            }

            return entries;
        }

        /// <summary>
        /// Maps Windows Event Log level values to our application's LogLevel
        /// </summary>
        /// <param name="eventLevel">Windows Event Log level</param>
        /// <returns>Corresponding log level in our application</returns>
        private LogLevel MapEventLevelToLogLevel(byte eventLevel)
        {
            return eventLevel switch
            {
                1 => LogLevel.Critical,  // Critical
                2 => LogLevel.Error,     // Error
                3 => LogLevel.Warning,   // Warning
                4 => LogLevel.Info,      // Information
                5 => LogLevel.Info,      // Verbose
                _ => LogLevel.Info       // Default to Info
            };
        }

        /// <summary>
        /// Gets a human-readable message from an Event Log record
        /// </summary>
        /// <param name="record">Event Log record</param>
        /// <returns>Formatted message</returns>
        private string GetEventMessage(EventRecord record)
        {
            try
            {
                return record.FormatDescription() ?? $"Event ID: {record.Id}";
            }
            catch
            {
                // If we can't format the description, return a simple message with the event ID
                return $"Event ID: {record.Id}";
            }
        }

        /// <summary>
        /// Gets critical system events from the past 24 hours
        /// </summary>
        /// <returns>List of critical log entries</returns>
        public List<LogEntry> GetCriticalEvents()
        {
            List<LogEntry> allEntries = GetRecentSystemEvents(500);

            // Filter for critical and error events from the last 24 hours
            return allEntries.Where(e =>
                (e.Level == LogLevel.Critical || e.Level == LogLevel.Error) &&
                e.Timestamp >= DateTime.Now.AddHours(-24)
            ).ToList();
        }

        /// <summary>
        /// Analyzes system logs for common issues
        /// </summary>
        /// <returns>List of detected issues</returns>
        public List<IssueInfo> AnalyzeSystemLogs()
        {
            List<IssueInfo> issues = new List<IssueInfo>();
            List<LogEntry> recentEvents = GetRecentSystemEvents(200);

            // Group events by provider to look for patterns
            var eventGroups = recentEvents
                .Where(e => e.Level == LogLevel.Error || e.Level == LogLevel.Critical)
                .GroupBy(e => e.Source)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Look for disk errors
            if (eventGroups.TryGetValue("System/disk", out var diskErrors) ||
                eventGroups.Any(g => g.Key.Contains("disk", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(new IssueInfo
                {
                    Icon = "💾",
                    Text = "Disk errors detected in system logs. This may indicate failing hardware.",
                    FixButtonText = "Check Disk",
                    FixActionTag = "DiskErrors",
                    IssueSeverity = IssueInfo.Severity.High,
                    Timestamp = DateTime.Now
                });
            }

            // Look for driver errors
            if (eventGroups.Any(g => g.Key.Contains("driver", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(new IssueInfo
                {
                    Icon = "🔧",
                    Text = "Driver issues detected. Update or reinstall hardware drivers to resolve.",
                    FixButtonText = "Device Manager",
                    FixActionTag = "DriverIssues",
                    IssueSeverity = IssueInfo.Severity.Medium,
                    Timestamp = DateTime.Now
                });
            }

            // Look for application crashes
            if (eventGroups.Any(g => g.Key.StartsWith("Application/") &&
                g.Value.Any(e => e.Message.Contains("crash", StringComparison.OrdinalIgnoreCase) ||
                                e.Message.Contains("stopped working", StringComparison.OrdinalIgnoreCase))))
            {
                issues.Add(new IssueInfo
                {
                    Icon = "❌",
                    Text = "Multiple application crashes detected. This may indicate system instability.",
                    FixButtonText = "System Scan",
                    FixActionTag = "AppCrashes",
                    IssueSeverity = IssueInfo.Severity.Medium,
                    Timestamp = DateTime.Now
                });
            }

            return issues;
        }
    }
}
using System;

namespace SysMax2._1.Models
{
    /// <summary>
    /// Enum representing different log levels
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// Model class representing a single log entry
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets or sets the timestamp when the log was created
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the log level
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Info;

        /// <summary>
        /// Gets or sets the source of the log (application, component, module)
        /// </summary>
        public string Source { get; set; } = "SysMax";

        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Gets or sets additional data associated with the log entry
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Creates a new empty log entry
        /// </summary>
        public LogEntry() { }

        /// <summary>
        /// Creates a new log entry with the specified values
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Log message</param>
        /// <param name="source">Log source</param>
        public LogEntry(LogLevel level, string message, string source = "SysMax")
        {
            Level = level;
            Message = message;
            Source = source;
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Returns a string representation of the log entry
        /// </summary>
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{Source}] {Message}";
        }
    }
}
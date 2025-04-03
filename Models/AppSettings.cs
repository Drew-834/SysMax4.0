using System;

namespace SysMax2._1.Models
{
    public class AppSettings
    {
        // General Settings
        public bool StartWithWindows { get; set; } = true;
        public bool RunInBackground { get; set; } = true;
        public string DefaultUserMode { get; set; } = "Basic"; // "Basic" or "Pro"
        public string Language { get; set; } = "English"; // Placeholder
        public string Theme { get; set; } = "Dark"; // Placeholder: "Dark", "Light", "System"

        // Notification Settings
        public bool EnableNotifications { get; set; } = true;
        public bool CriticalIssuesOnly { get; set; } = false;
        public bool NotificationSound { get; set; } = true;

        // Monitoring Settings
        public int UpdateFrequencySeconds { get; set; } = 2;
        public int CpuAlertThreshold { get; set; } = 80;
        public int MemoryAlertThreshold { get; set; } = 85;
        public int DiskAlertThreshold { get; set; } = 90;

        // Advanced Settings
        public bool EnableLogging { get; set; } = true;
        public bool AutoUpdateCheck { get; set; } = true; // Placeholder

        // Helper method to get ComboBox index from frequency value
        public int GetUpdateFrequencyIndex()
        {
            return UpdateFrequencySeconds switch
            {
                1 => 0,
                2 => 1,
                5 => 2,
                10 => 3,
                _ => 1 // Default to 2 seconds
            };
        }

        // Helper method to set frequency value from ComboBox index
        public void SetUpdateFrequencyFromIndex(int index)
        {
            UpdateFrequencySeconds = index switch
            {
                0 => 1,
                1 => 2,
                2 => 5,
                3 => 10,
                _ => 2 // Default to 2 seconds
            };
        }
         // Helper method to get ComboBox index from theme value
        public int GetThemeIndex()
        {
            return Theme.ToLower() switch
            {
                "dark" => 0,
                "light" => 1,
                "system" => 2,
                _ => 0 // Default to Dark
            };
        }

        // Helper method to set theme value from ComboBox index
        public void SetThemeFromIndex(int index)
        {
            Theme = index switch
            {
                0 => "Dark",
                1 => "Light",
                2 => "System",
                _ => "Dark" // Default to Dark
            };
        }
         // Helper method to get ComboBox index from user mode value
        public int GetUserModeIndex()
        {
            return DefaultUserMode.ToLower() switch
            {
                "basic" => 0,
                "pro" => 1,
                _ => 0 // Default to Basic
            };
        }

        // Helper method to set user mode value from ComboBox index
        public void SetUserModeFromIndex(int index)
        {
            DefaultUserMode = index switch
            {
                0 => "Basic",
                1 => "Pro",
                _ => "Basic" // Default to Basic
            };
        }
    }
} 
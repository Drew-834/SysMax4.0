using System;
using System.IO;
using System.Text.Json;
using SysMax2._1.Models;

namespace SysMax2._1.Services
{
    public class SettingsService
    {
        private static readonly LoggingService _loggingService = LoggingService.Instance; // Get LoggingService instance
        private static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string SettingsFolderPath = Path.Combine(AppDataFolder, "SysMax");
        private static readonly string SettingsFilePath = Path.Combine(SettingsFolderPath, "settings.json");

        // Singleton pattern
        private static readonly Lazy<SettingsService> _instance = new Lazy<SettingsService>(() => new SettingsService());
        public static SettingsService Instance => _instance.Value;

        // Event raised when settings are saved
        public event EventHandler? SettingsSaved;

        private SettingsService() 
        {
             // Private constructor for singleton
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    _loggingService.Log(LogLevel.Info, "Settings file not found. Returning default settings.");
                    return new AppSettings();
                }

                string json = File.ReadAllText(SettingsFilePath);
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings == null)
                {
                    _loggingService.Log(LogLevel.Warning, "Failed to deserialize settings. Returning default settings.");
                    return new AppSettings();
                }
                 _loggingService.Log(LogLevel.Info, "Settings loaded successfully.");
                return settings;
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading settings: {ex.Message}");
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(SettingsFolderPath);
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
                _loggingService.Log(LogLevel.Info, "Settings saved successfully.");

                // Raise the event
                OnSettingsSaved();
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error saving settings: {ex.Message}");
                // Consider re-throwing or notifying user based on severity
            }
        }

        protected virtual void OnSettingsSaved()
        {
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        public void ResetToDefaults()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    File.Delete(SettingsFilePath);
                    _loggingService.Log(LogLevel.Info, "Settings file deleted successfully.");
                }
                else
                {
                    _loggingService.Log(LogLevel.Info, "Settings file did not exist. No action needed for reset.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error deleting settings file: {ex.Message}");
                // Consider re-throwing or notifying user
            }
        }
    }
} 
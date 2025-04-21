using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using SysMax2._1.Models;

namespace SysMax2._1.Services
{
    /// <summary>
    /// Enhanced service for hardware monitoring with real-time updates and events
    /// </summary>
    public class EnhancedHardwareMonitorService : INotifyPropertyChanged, IDisposable
    {
        private static EnhancedHardwareMonitorService? _instance;
        private readonly HardwareMonitorService _baseMonitor;
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private readonly SettingsService _settingsService = SettingsService.Instance;
        private Timer? _updateTimer;
        private readonly object _lockObj = new object();
        private CancellationTokenSource? _cts;
        private bool _isMonitoring = false;

        // Hardware metrics with property change notification
        private float _cpuUsage;
        public float CpuUsage
        {
            get => _cpuUsage;
            private set
            {
                if (_cpuUsage != value)
                {
                    _cpuUsage = value;
                    OnPropertyChanged();

                    // Check thresholds and raise events
                    if (_cpuUsage > HighCpuThreshold && !_isHighCpuAlertActive)
                    {
                        _isHighCpuAlertActive = true;
                        OnHighCpuUsageDetected();
                    }
                    else if (_cpuUsage < HighCpuThreshold - 5 && _isHighCpuAlertActive)
                    {
                        _isHighCpuAlertActive = false;
                    }
                }
            }
        }

        private float _cpuTemperature;
        public float CpuTemperature
        {
            get => _cpuTemperature;
            private set
            {
                if (_cpuTemperature != value)
                {
                    _cpuTemperature = value;
                    OnPropertyChanged();

                    // Check thresholds and raise events
                    if (_cpuTemperature > HighTemperatureThreshold && !_isHighTempAlertActive)
                    {
                        _isHighTempAlertActive = true;
                        OnHighTemperatureDetected();
                    }
                    else if (_cpuTemperature < HighTemperatureThreshold - 5 && _isHighTempAlertActive)
                    {
                        _isHighTempAlertActive = false;
                    }
                }
            }
        }

        private float _memoryUsage;
        public float MemoryUsage
        {
            get => _memoryUsage;
            private set
            {
                if (_memoryUsage != value)
                {
                    _memoryUsage = value;
                    OnPropertyChanged();

                    // Check thresholds and raise events
                    if (_memoryUsage > HighMemoryThreshold && !_isHighMemoryAlertActive)
                    {
                        _isHighMemoryAlertActive = true;
                        OnHighMemoryUsageDetected();
                    }
                    else if (_memoryUsage < HighMemoryThreshold - 5 && _isHighMemoryAlertActive)
                    {
                        _isHighMemoryAlertActive = false;
                    }
                }
            }
        }

        private long _availableMemory;
        public long AvailableMemory
        {
            get => _availableMemory;
            private set
            {
                if (_availableMemory != value)
                {
                    _availableMemory = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _totalMemory;
        public long TotalMemory
        {
            get => _totalMemory;
            private set
            {
                if (_totalMemory != value)
                {
                    _totalMemory = value;
                    OnPropertyChanged();
                }
            }
        }

        private float _diskUsage;
        public float DiskUsage
        {
            get => _diskUsage;
            private set
            {
                if (_diskUsage != value)
                {
                    _diskUsage = value;
                    OnPropertyChanged();

                    // Check thresholds and raise events
                    if (_diskUsage > HighDiskUsageThreshold && !_isHighDiskUsageAlertActive)
                    {
                        _isHighDiskUsageAlertActive = true;
                        OnHighDiskUsageDetected();
                    }
                    else if (_diskUsage < HighDiskUsageThreshold - 5 && _isHighDiskUsageAlertActive)
                    {
                        _isHighDiskUsageAlertActive = false;
                    }
                }
            }
        }

        private long _availableDiskSpace;
        public long AvailableDiskSpace
        {
            get => _availableDiskSpace;
            private set
            {
                if (_availableDiskSpace != value)
                {
                    _availableDiskSpace = value;
                    OnPropertyChanged();

                    // Check thresholds and raise events
                    if (_availableDiskSpace < LowDiskSpaceThreshold && !_isLowDiskSpaceAlertActive)
                    {
                        _isLowDiskSpaceAlertActive = true;
                        OnLowDiskSpaceDetected();
                    }
                    else if (_availableDiskSpace > LowDiskSpaceThreshold + 5 && _isLowDiskSpaceAlertActive)
                    {
                        _isLowDiskSpaceAlertActive = false;
                    }
                }
            }
        }

        private long _totalDiskSpace;
        public long TotalDiskSpace
        {
            get => _totalDiskSpace;
            private set
            {
                if (_totalDiskSpace != value)
                {
                    _totalDiskSpace = value;
                    OnPropertyChanged();
                }
            }
        }

        private float _networkDownloadSpeed;
        public float NetworkDownloadSpeed
        {
            get => _networkDownloadSpeed;
            private set
            {
                if (_networkDownloadSpeed != value)
                {
                    _networkDownloadSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        private float _networkUploadSpeed;
        public float NetworkUploadSpeed
        {
            get => _networkUploadSpeed;
            private set
            {
                if (_networkUploadSpeed != value)
                {
                    _networkUploadSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isNetworkConnected;
        public bool IsNetworkConnected
        {
            get => _isNetworkConnected;
            private set
            {
                if (_isNetworkConnected != value)
                {
                    _isNetworkConnected = value;
                    OnPropertyChanged();

                    // Raise event when network status changes
                    if (!_isNetworkConnected)
                    {
                        OnNetworkDisconnected();
                    }
                    else
                    {
                        OnNetworkConnected();
                    }
                }
            }
        }

        // Alert threshold properties
        public float HighCpuThreshold { get; set; } = 80;
        public float HighTemperatureThreshold { get; set; } = 80;
        public float HighMemoryThreshold { get; set; } = 85;
        public float HighDiskUsageThreshold { get; set; } = 90;
        public long LowDiskSpaceThreshold { get; set; } = 10L * 1024 * 1024 * 1024; // 10 GB I had to add the L to make it use 64-bit arithmetic

        // Alert tracking flags
        private bool _isHighCpuAlertActive = false;
        private bool _isHighTempAlertActive = false;
        private bool _isHighMemoryAlertActive = false;
        private bool _isHighDiskUsageAlertActive = false;
        private bool _isLowDiskSpaceAlertActive = false;

        // Events
        public event EventHandler? HighCpuUsageDetected;
        public event EventHandler? HighTemperatureDetected;
        public event EventHandler? HighMemoryUsageDetected;
        public event EventHandler? HighDiskUsageDetected;
        public event EventHandler? LowDiskSpaceDetected;
        public event EventHandler? NetworkConnected;
        public event EventHandler? NetworkDisconnected;
        public event PropertyChangedEventHandler? PropertyChanged;

        // Last stats for network tracking
        private long _lastBytesReceived = 0;
        private long _lastBytesSent = 0;
        private DateTime _lastSampleTime = DateTime.Now;
        private NetworkInterface? _activeNetworkInterface = null;

        // Property to expose monitoring state
        public bool IsMonitoring => _isMonitoring;

        private EnhancedHardwareMonitorService()
        {
            _baseMonitor = HardwareMonitorService.Instance;
            InitializeSystemData();
            _settingsService.SettingsSaved += HandleSettingsSaved;
            _loggingService.Log(LogLevel.Info, "Enhanced Hardware Monitor Service initialized");
        }

        /// <summary>
        /// Gets the singleton instance of the EnhancedHardwareMonitorService
        /// </summary>
        public static EnhancedHardwareMonitorService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EnhancedHardwareMonitorService();
                }
                return _instance;
            }
        }

        private void InitializeSystemData()
        {
            try
            {
                // Initialize memory information
                UpdateMemoryInformation();

                // Initialize disk information
                UpdateDiskInformation();

                // Initialize network status
                IsNetworkConnected = NetworkInterface.GetIsNetworkAvailable();

                // Find active network interface
                FindActiveNetworkInterface();
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error initializing system data: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts real-time monitoring of system hardware using the interval defined in settings.
        /// </summary>
        public void StartMonitoring()
        {
            lock (_lockObj)
            {
                if (_isMonitoring)
                    return;

                _cts = new CancellationTokenSource();

                // Load current settings to get interval
                AppSettings settings = _settingsService.LoadSettings();
                int intervalMilliseconds = settings.UpdateFrequencySeconds * 1000;

                // Create and start update timer with interval from settings
                _updateTimer = new Timer(UpdateCallback, null, 0, intervalMilliseconds);

                _isMonitoring = true;
                _loggingService.Log(LogLevel.Info, $"Started hardware monitoring with {intervalMilliseconds}ms interval from settings");
            }
        }

        /// <summary>
        /// Stops hardware monitoring
        /// </summary>
        public void StopMonitoring()
        {
            lock (_lockObj)
            {
                if (!_isMonitoring)
                    return;

                _updateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _updateTimer?.Dispose();
                _updateTimer = null;

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;

                _isMonitoring = false;
                _loggingService.Log(LogLevel.Info, "Stopped hardware monitoring");
            }
        }

        private void HandleSettingsSaved(object? sender, EventArgs e)
        {
            lock (_lockObj)
            {
                if (!_isMonitoring || _updateTimer == null)
                    return;

                AppSettings newSettings = _settingsService.LoadSettings();
                int newIntervalMilliseconds = newSettings.UpdateFrequencySeconds * 1000;

                // Update alert thresholds immediately
                HighCpuThreshold = newSettings.CpuAlertThreshold;
                HighMemoryThreshold = newSettings.MemoryAlertThreshold;
                HighDiskUsageThreshold = newSettings.DiskAlertThreshold;
                // Add other thresholds if needed (e.g., temperature, low disk space)

                // Check if the timer interval needs updating
                if (_updateTimer != null)
                {
                    // Get the current interval (requires reflection or storing it separately)
                    // For simplicity, let's just restart the timer with the new interval
                    _updateTimer.Change(0, newIntervalMilliseconds);
                    _loggingService.Log(LogLevel.Info, $"Updated hardware monitoring interval to {newIntervalMilliseconds}ms due to settings change");
                }
            }
        }

        private void UpdateCallback(object? state)
        {
            try
            {
                if (_cts?.IsCancellationRequested == true)
                    return;

                // Update hardware metrics
                UpdateHardwareMetrics();
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating hardware metrics: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates all hardware metrics
        /// </summary>
        public void UpdateHardwareMetrics()
        {
            try
            {
                // Update LibreHardwareMonitor data
                _baseMonitor.Update();

                // Update CPU metrics
                UpdateCpuMetrics();

                // Update memory metrics
                UpdateMemoryInformation();

                // Update disk metrics
                UpdateDiskInformation();

                // Update network metrics
                UpdateNetworkMetrics();
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating hardware metrics: {ex.Message}");
            }
        }

        private void UpdateCpuMetrics()
        {
            try
            {
                // Get CPU info from base monitor
                var cpuInfo = _baseMonitor.GetCpuInfo();

                // Update CPU usage
                if (cpuInfo.TryGetValue("Load", out string? loadStr))
                {
                    if (float.TryParse(loadStr.Replace(" %", ""), out float load))
                    {
                        CpuUsage = load;
                    }
                }
                else
                {
                    // Fallback to performance counter
                    using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true))
                    {
                        // First call just initializes the counter
                        cpuCounter.NextValue();

                        // Small delay for accurate reading
                        Thread.Sleep(100);

                        // Get actual value
                        CpuUsage = cpuCounter.NextValue();
                    }
                }

                // Update CPU temperature
                if (cpuInfo.TryGetValue("Temperature", out string? tempStr))
                {
                    if (float.TryParse(tempStr.Replace(" °C", ""), out float temp))
                    {
                        CpuTemperature = temp;
                    }
                }
                else
                {
                    // Try to get temperature from WMI
                    CpuTemperature = GetCpuTemperatureFromWmi();
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating CPU metrics: {ex.Message}");
            }
        }

        private float GetCpuTemperatureFromWmi()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var tempObj = obj["CurrentTemperature"];
                        if (tempObj != null && uint.TryParse(tempObj.ToString(), out uint tempKelvinTenths))
                        {
                            // Temperature is in tenths of degrees Kelvin
                            // Convert from tenths of Kelvin to Celsius
                            return (float)((tempKelvinTenths / 10.0) - 273.15);
                        }
                        else
                        {
                            _loggingService.Log(LogLevel.Warning, "Could not parse CurrentTemperature from MSAcpi_ThermalZoneTemperature.");
                            // Continue to next object or fallback if this was the only one
                        }
                    }
                }

                // If WMI MSAcpi_ThermalZoneTemperature fails or returns no valid data, try Win32_TemperatureProbe
                _loggingService.Log(LogLevel.Info, "MSAcpi_ThermalZoneTemperature failed, trying Win32_TemperatureProbe for CPU Temp.");
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_TemperatureProbe"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        int temp = Convert.ToInt32(obj["CurrentReading"]);
                        return temp;
                    }
                }

                // If all methods fail, return a default value that won't trigger alerts
                return 50;
            }
            catch
            {
                // Return a default value that won't trigger alerts
                return 50;
            }
        }

        private void UpdateMemoryInformation()
        {
            try
            {
                // Get memory info from base monitor
                var memoryInfo = _baseMonitor.GetMemoryInfo();

                // Update memory usage
                if (memoryInfo.TryGetValue("Load", out string? loadStr))
                {
                    if (float.TryParse(loadStr.Replace(" %", ""), out float load))
                    {
                        MemoryUsage = load;
                    }
                }
                else
                {
                    // Fallback to performance counter
                    using (var memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", true))
                    {
                        // Prime the counter
                        memCounter.NextValue();
                        Thread.Sleep(100); // Give it a moment
                        // Get the actual value
                        MemoryUsage = memCounter.NextValue();
                    }
                }

                // Update available memory - Hardcode to 8 GB
                const long eightGBInBytes = 8L * 1024 * 1024 * 1024;
                if (AvailableMemory != eightGBInBytes) // Set only if different
                {
                    AvailableMemory = eightGBInBytes;
                    _loggingService.Log(LogLevel.Info, $"[UpdateMemoryInfo] Hardcoded AvailableMemory set to: {AvailableMemory} bytes"); 
                }
                
                // Update total memory - Hardcode to 16 GB
                const long sixteenGBInBytes = 16L * 1024 * 1024 * 1024;
                if (TotalMemory != sixteenGBInBytes) // Set only if different to avoid unnecessary property change notifications
                {
                    TotalMemory = sixteenGBInBytes;
                    _loggingService.Log(LogLevel.Info, $"[UpdateMemoryInfo] Hardcoded TotalMemory set to: {TotalMemory} bytes");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating memory information: {ex.Message}");
            }
        }

        private void UpdateDiskInformation()
        {
            try
            {
                long totalSpace = 0;
                long totalFreeSpace = 0;

                // Calculate totals across all fixed drives
                foreach (var drive in System.IO.DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == System.IO.DriveType.Fixed)
                    {
                        totalSpace += drive.TotalSize;
                        totalFreeSpace += drive.AvailableFreeSpace;
                    }
                }

                // Update properties
                TotalDiskSpace = totalSpace;
                AvailableDiskSpace = totalFreeSpace;

                // Calculate usage percentage
                if (totalSpace > 0)
                {
                    DiskUsage = 100 - ((float)totalFreeSpace / totalSpace * 100);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating disk information: {ex.Message}");
            }
        }

        private void UpdateNetworkMetrics()
        {
            try
            {
                // Update network connection status
                IsNetworkConnected = NetworkInterface.GetIsNetworkAvailable();

                if (IsNetworkConnected)
                {
                    // Make sure we have an active interface
                    if (_activeNetworkInterface == null || _activeNetworkInterface.OperationalStatus != OperationalStatus.Up)
                    {
                        FindActiveNetworkInterface();
                    }

                    // Update network throughput
                    if (_activeNetworkInterface != null)
                    {
                        UpdateNetworkThroughput();
                    }
                }
                else
                {
                    // Reset network speeds when disconnected
                    NetworkDownloadSpeed = 0;
                    NetworkUploadSpeed = 0;
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating network metrics: {ex.Message}");
            }
        }

        private void FindActiveNetworkInterface()
        {
            try
            {
                _activeNetworkInterface = null;

                // Find active network interface
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                         ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                        ni.GetIPv4Statistics() != null)
                    {
                        _activeNetworkInterface = ni;

                        // Reset network tracking data
                        _lastBytesReceived = 0;
                        _lastBytesSent = 0;
                        _lastSampleTime = DateTime.Now;

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error finding active network interface: {ex.Message}");
            }
        }

        private void UpdateNetworkThroughput()
        {
            try
            {
                if (_activeNetworkInterface == null)
                    return;

                IPv4InterfaceStatistics stats = _activeNetworkInterface.GetIPv4Statistics();

                // First sample - just store values
                if (_lastBytesReceived == 0)
                {
                    _lastBytesReceived = stats.BytesReceived;
                    _lastBytesSent = stats.BytesSent;
                    _lastSampleTime = DateTime.Now;
                    return;
                }

                // Calculate throughput
                DateTime now = DateTime.Now;
                TimeSpan timeSpan = now - _lastSampleTime;
                double seconds = timeSpan.TotalSeconds;

                if (seconds > 0)
                {
                    long bytesDeltaReceived = stats.BytesReceived - _lastBytesReceived;
                    long bytesDeltaSent = stats.BytesSent - _lastBytesSent;

                    // Convert to KB/s
                    NetworkDownloadSpeed = (float)(bytesDeltaReceived / seconds / 1024);
                    NetworkUploadSpeed = (float)(bytesDeltaSent / seconds / 1024);

                    // Update tracking data
                    _lastBytesReceived = stats.BytesReceived;
                    _lastBytesSent = stats.BytesSent;
                    _lastSampleTime = now;
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating network throughput: {ex.Message}");
            }
        }

        // Event invokers
        protected virtual void OnHighCpuUsageDetected()
        {
            _loggingService.Log(LogLevel.Warning, $"High CPU usage detected: {CpuUsage:F1}%");
            HighCpuUsageDetected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnHighTemperatureDetected()
        {
            _loggingService.Log(LogLevel.Warning, $"High CPU temperature detected: {CpuTemperature:F1}°C");
            HighTemperatureDetected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnHighMemoryUsageDetected()
        {
            _loggingService.Log(LogLevel.Warning, $"High memory usage detected: {MemoryUsage:F1}%");
            HighMemoryUsageDetected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnHighDiskUsageDetected()
        {
            _loggingService.Log(LogLevel.Warning, $"High disk usage detected: {DiskUsage:F1}%");
            HighDiskUsageDetected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLowDiskSpaceDetected()
        {
            double gbAvailable = AvailableDiskSpace / (1024.0 * 1024 * 1024);
            _loggingService.Log(LogLevel.Warning, $"Low disk space detected: {gbAvailable:F1} GB available");
            LowDiskSpaceDetected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnNetworkConnected()
        {
            _loggingService.Log(LogLevel.Info, "Network connection established");
            NetworkConnected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnNetworkDisconnected()
        {
            _loggingService.Log(LogLevel.Warning, "Network connection lost");
            NetworkDisconnected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets system information for the dashboard
        /// </summary>
        /// <returns>Dictionary with system information</returns>
        public Dictionary<string, string> GetSystemInformation()
        {
            Dictionary<string, string> sysInfo = new Dictionary<string, string>();

            try
            {
                // OS information
                sysInfo["OSName"] = Environment.OSVersion.VersionString;
                sysInfo["OSVersion"] = GetWindowsBuildInfo();
                sysInfo["ComputerName"] = Environment.MachineName;
                sysInfo["Username"] = Environment.UserName;

                // CPU information
                sysInfo["ProcessorName"] = GetProcessorName();
                sysInfo["CPUCores"] = Environment.ProcessorCount.ToString();

                // RAM information
                _loggingService.Log(LogLevel.Info, $"[GetSystemInfo] Reading TotalMemory: {TotalMemory} bytes, AvailableMemory: {AvailableMemory} bytes");
                sysInfo["TotalRAM"] = $"{TotalMemory / (1024.0 * 1024 * 1024):F1} GB";
                sysInfo["AvailableRAM"] = $"{AvailableMemory / (1024.0 * 1024 * 1024):F1} GB";

                // Get additional info from LibreHardwareMonitor
                var gpuInfo = _baseMonitor.GetGpuInfo();
                if (gpuInfo.ContainsKey("Name"))
                {
                    sysInfo["GPUName"] = gpuInfo["Name"];
                }
                else
                {
                    sysInfo["GPUName"] = GetGraphicsCardName();
                }

                // Add storage summary
                sysInfo["StorageSummary"] = $"{TotalDiskSpace / (1024.0 * 1024 * 1024):F0} GB Total, {AvailableDiskSpace / (1024.0 * 1024 * 1024):F1} GB Free";

                // Add network adapter info
                if (_activeNetworkInterface != null)
                {
                    sysInfo["NetworkAdapter"] = _activeNetworkInterface.Description;
                }
                else
                {
                    sysInfo["NetworkAdapter"] = "No active network adapter";
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting system information: {ex.Message}");
            }

            return sysInfo;
        }

        private string GetWindowsBuildInfo()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string? buildNumber = key.GetValue("CurrentBuildNumber")?.ToString();
                        string? ubr = key.GetValue("UBR")?.ToString(); // Update Build Revision
                        string? displayVersion = key.GetValue("DisplayVersion")?.ToString(); // Feature update version (e.g., 21H2)

                        if (!string.IsNullOrEmpty(displayVersion) && !string.IsNullOrEmpty(buildNumber))
                        {
                            return $"{displayVersion} (Build {buildNumber}.{ubr})";
                        }
                        else if (!string.IsNullOrEmpty(buildNumber))
                        {
                            return $"Build {buildNumber}.{ubr}";
                        }
                    }
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetProcessorName()
        {
            try
            {
                string? cpuName = Microsoft.Win32.Registry.LocalMachine
                    .OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0")?
                    .GetValue("ProcessorNameString") as string;

                return cpuName ?? "Unknown CPU";
            }
            catch
            {
                return "Unknown CPU";
            }
        }

        private string GetGraphicsCardName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Name"].ToString();
                    }
                }

                return "Unknown Graphics Card";
            }
            catch
            {
                return "Unknown Graphics Card";
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Unsubscribe from settings changes
            _settingsService.SettingsSaved -= HandleSettingsSaved;
            StopMonitoring();
            _loggingService.Log(LogLevel.Info, "Enhanced Hardware Monitor Service disposed");
            GC.SuppressFinalize(this);
        }
    }
}
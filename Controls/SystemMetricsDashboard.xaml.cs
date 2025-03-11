using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SysMax2._1.Models;
using SysMax2._1.Services;


namespace SysMax2._1.Controls
{
    /// <summary>
    /// Interaction logic for SystemMetricsDashboard.xaml
    /// </summary>
    public partial class SystemMetricsDashboard : UserControl, INotifyPropertyChanged
    {
        private readonly EnhancedHardwareMonitorService _hardwareMonitor;
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private DispatcherTimer _updateTimer;

        // Properties for data binding
        private float _cpuUsage;
        public float CpuUsage
        {
            get => _cpuUsage;
            set
            {
                if (_cpuUsage != value)
                {
                    _cpuUsage = value;
                    UpdateCpuHealthIndicator();
                    OnPropertyChanged();
                }
            }
        }

        private float _cpuTemperature;
        public float CpuTemperature
        {
            get => _cpuTemperature;
            set
            {
                if (_cpuTemperature != value)
                {
                    _cpuTemperature = value;
                    OnPropertyChanged();
                }
            }
        }

        private float _memoryUsage;
        public float MemoryUsage
        {
            get => _memoryUsage;
            set
            {
                if (_memoryUsage != value)
                {
                    _memoryUsage = value;
                    UpdateMemoryHealthIndicator();
                    OnPropertyChanged();
                }
            }
        }

        private long _availableMemory;
        public long AvailableMemory
        {
            get => _availableMemory;
            set
            {
                if (_availableMemory != value)
                {
                    _availableMemory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AvailableMemoryDisplay));
                }
            }
        }

        public string AvailableMemoryDisplay => $"{AvailableMemory / (1024.0 * 1024 * 1024):F1} GB Free";

        private float _diskUsage;
        public float DiskUsage
        {
            get => _diskUsage;
            set
            {
                if (_diskUsage != value)
                {
                    _diskUsage = value;
                    UpdateDiskHealthIndicator();
                    OnPropertyChanged();
                }
            }
        }

        private long _availableDiskSpace;
        public long AvailableDiskSpace
        {
            get => _availableDiskSpace;
            set
            {
                if (_availableDiskSpace != value)
                {
                    _availableDiskSpace = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AvailableDiskSpaceDisplay));
                }
            }
        }

        public string AvailableDiskSpaceDisplay => $"{AvailableDiskSpace / (1024.0 * 1024 * 1024):F1} GB Free";

        private float _networkDownloadSpeed;
        public float NetworkDownloadSpeed
        {
            get => _networkDownloadSpeed;
            set
            {
                if (_networkDownloadSpeed != value)
                {
                    _networkDownloadSpeed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetworkDisplayString));
                }
            }
        }

        private float _networkUploadSpeed;
        public float NetworkUploadSpeed
        {
            get => _networkUploadSpeed;
            set
            {
                if (_networkUploadSpeed != value)
                {
                    _networkUploadSpeed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetworkDisplayString));
                }
            }
        }

        private bool _isNetworkConnected;
        public bool IsNetworkConnected
        {
            get => _isNetworkConnected;
            set
            {
                if (_isNetworkConnected != value)
                {
                    _isNetworkConnected = value;
                    UpdateNetworkHealthIndicator();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetworkStatus));
                }
            }
        }

        public string NetworkStatus => IsNetworkConnected ? "Connected" : "Disconnected";

        public string NetworkDisplayString
        {
            get
            {
                if (!IsNetworkConnected)
                    return "Offline";

                string downloadUnit = "KB/s";
                string uploadUnit = "KB/s";
                float displayDownload = NetworkDownloadSpeed;
                float displayUpload = NetworkUploadSpeed;

                // Convert to MB/s if speeds are high enough
                if (NetworkDownloadSpeed > 1024)
                {
                    displayDownload = NetworkDownloadSpeed / 1024;
                    downloadUnit = "MB/s";
                }

                if (NetworkUploadSpeed > 1024)
                {
                    displayUpload = NetworkUploadSpeed / 1024;
                    uploadUnit = "MB/s";
                }

                return $"↓{displayDownload:F1} {downloadUnit}\n↑{displayUpload:F1} {uploadUnit}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public SystemMetricsDashboard()
        {
            InitializeComponent();

            // Set DataContext to this for binding
            DataContext = this;

            // Get hardware monitor service
            _hardwareMonitor = EnhancedHardwareMonitorService.Instance;

            // Initialize with current values
            UpdateFromHardwareMonitor();

            // Set up update timer
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // Start hardware monitoring if not already running
            if (!_hardwareMonitor.IsMonitoring)
            {
                _hardwareMonitor.StartMonitoring();
            }

            // Subscribe to hardware events
            _hardwareMonitor.PropertyChanged += HardwareMonitor_PropertyChanged;

            // Register for unloaded event to clean up resources
            this.Unloaded += UserControl_Unloaded;

            // Log initialization
            _loggingService.Log(LogLevel.Info, "System Metrics Dashboard initialized");
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateFromHardwareMonitor();
        }

        private void HardwareMonitor_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Update specific property when it changes in the hardware monitor
            switch (e.PropertyName)
            {
                case nameof(EnhancedHardwareMonitorService.CpuUsage):
                    CpuUsage = _hardwareMonitor.CpuUsage;
                    break;
                case nameof(EnhancedHardwareMonitorService.CpuTemperature):
                    CpuTemperature = _hardwareMonitor.CpuTemperature;
                    break;
                case nameof(EnhancedHardwareMonitorService.MemoryUsage):
                    MemoryUsage = _hardwareMonitor.MemoryUsage;
                    break;
                case nameof(EnhancedHardwareMonitorService.AvailableMemory):
                    AvailableMemory = _hardwareMonitor.AvailableMemory;
                    break;
                case nameof(EnhancedHardwareMonitorService.DiskUsage):
                    DiskUsage = _hardwareMonitor.DiskUsage;
                    break;
                case nameof(EnhancedHardwareMonitorService.AvailableDiskSpace):
                    AvailableDiskSpace = _hardwareMonitor.AvailableDiskSpace;
                    break;
                case nameof(EnhancedHardwareMonitorService.NetworkDownloadSpeed):
                    NetworkDownloadSpeed = _hardwareMonitor.NetworkDownloadSpeed;
                    break;
                case nameof(EnhancedHardwareMonitorService.NetworkUploadSpeed):
                    NetworkUploadSpeed = _hardwareMonitor.NetworkUploadSpeed;
                    break;
                case nameof(EnhancedHardwareMonitorService.IsNetworkConnected):
                    IsNetworkConnected = _hardwareMonitor.IsNetworkConnected;
                    break;
            }
        }

        private void UpdateFromHardwareMonitor()
        {
            CpuUsage = _hardwareMonitor?.CpuUsage ?? 0;
            CpuTemperature = _hardwareMonitor?.CpuTemperature ?? 0;
            MemoryUsage = _hardwareMonitor?.MemoryUsage ?? 0;
            AvailableMemory = _hardwareMonitor?.AvailableMemory ?? 0;
            DiskUsage = _hardwareMonitor?.DiskUsage ?? 0;
            AvailableDiskSpace = _hardwareMonitor?.AvailableDiskSpace ?? 0;
            NetworkDownloadSpeed = _hardwareMonitor?.NetworkDownloadSpeed ?? 0;
            NetworkUploadSpeed = _hardwareMonitor?.NetworkUploadSpeed ?? 0;
            IsNetworkConnected = _hardwareMonitor?.IsNetworkConnected ?? false;
        }

        private void UpdateCpuHealthIndicator()
        {
            if (CpuUsage > 90 || CpuTemperature > 85)
            {
                CpuHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else if (CpuUsage > 70 || CpuTemperature > 75)
            {
                CpuHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                CpuHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }
        }

        private void UpdateMemoryHealthIndicator()
        {
            if (MemoryUsage > 90)
            {
                MemoryHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else if (MemoryUsage > 75)
            {
                MemoryHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                MemoryHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }
        }

        private void UpdateDiskHealthIndicator()
        {
            if (DiskUsage > 90)
            {
                DiskHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else if (DiskUsage > 75)
            {
                DiskHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                DiskHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }
        }

        private void UpdateNetworkHealthIndicator()
        {
            if (!IsNetworkConnected)
            {
                NetworkHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else
            {
                NetworkHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopMonitoring();
        }

        public void StopMonitoring()
        {
            // Stop the update timer (no need to dispose as DispatcherTimer doesn't implement IDisposable)
            _updateTimer?.Stop();

            // Unsubscribe from events
            _hardwareMonitor.PropertyChanged -= HardwareMonitor_PropertyChanged;
        }
    }
}
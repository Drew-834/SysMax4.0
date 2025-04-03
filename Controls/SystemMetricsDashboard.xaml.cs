using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

            // Initialize with current values immediately
            UpdateFromHardwareMonitor();

            // Start hardware monitoring if not already running (now uses setting interval)
            if (!_hardwareMonitor.IsMonitoring)
            {
                // No interval needed here, StartMonitoring reads from settings
                _hardwareMonitor.StartMonitoring();
            }

            // Subscribe to hardware events
            _hardwareMonitor.PropertyChanged += HardwareMonitor_PropertyChanged;

            // Register for unloaded event to clean up resources
            this.Unloaded += UserControl_Unloaded;

            // Log initialization
            _loggingService.Log(LogLevel.Info, "System Metrics Dashboard initialized and subscribed to hardware events");
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
            CpuUsage = _hardwareMonitor.CpuUsage;
            CpuTemperature = _hardwareMonitor.CpuTemperature;
            MemoryUsage = _hardwareMonitor.MemoryUsage;
            AvailableMemory = _hardwareMonitor.AvailableMemory;
            DiskUsage = _hardwareMonitor.DiskUsage;
            AvailableDiskSpace = _hardwareMonitor.AvailableDiskSpace;
            NetworkDownloadSpeed = _hardwareMonitor.NetworkDownloadSpeed;
            NetworkUploadSpeed = _hardwareMonitor.NetworkUploadSpeed;
            IsNetworkConnected = _hardwareMonitor.IsNetworkConnected;
            // Ensure indicators are updated based on initial values
            UpdateCpuHealthIndicator();
            UpdateMemoryHealthIndicator();
            UpdateDiskHealthIndicator();
            UpdateNetworkHealthIndicator();
        }

        private void UpdateCpuHealthIndicator()
        {
            if (CpuUsage > 90 || CpuTemperature > 85)
            {
                CpuHealthIndicator.Fill = Application.Current.FindResource("DangerColor") as SolidColorBrush;
            }
            else if (CpuUsage > 70 || CpuTemperature > 75)
            {
                CpuHealthIndicator.Fill = Application.Current.FindResource("WarningColor") as SolidColorBrush;
            }
            else
            {
                CpuHealthIndicator.Fill = Application.Current.FindResource("SecondaryColor") as SolidColorBrush;
            }
        }

        private void UpdateMemoryHealthIndicator()
        {
            if (MemoryUsage > 90)
            {
                MemoryHealthIndicator.Fill = Application.Current.FindResource("DangerColor") as SolidColorBrush;
            }
            else if (MemoryUsage > 75)
            {
                MemoryHealthIndicator.Fill = Application.Current.FindResource("WarningColor") as SolidColorBrush;
            }
            else
            {
                MemoryHealthIndicator.Fill = Application.Current.FindResource("SecondaryColor") as SolidColorBrush;
            }
        }

        private void UpdateDiskHealthIndicator()
        {
            if (DiskUsage > 90)
            {
                DiskHealthIndicator.Fill = Application.Current.FindResource("DangerColor") as SolidColorBrush;
            }
            else if (DiskUsage > 75)
            {
                DiskHealthIndicator.Fill = Application.Current.FindResource("WarningColor") as SolidColorBrush;
            }
            else
            {
                DiskHealthIndicator.Fill = Application.Current.FindResource("SecondaryColor") as SolidColorBrush;
            }
        }

        private void UpdateNetworkHealthIndicator()
        {
            if (!IsNetworkConnected)
            {
                NetworkHealthIndicator.Fill = Application.Current.FindResource("DangerColor") as SolidColorBrush;
            }
            else
            {
                NetworkHealthIndicator.Fill = Application.Current.FindResource("SecondaryColor") as SolidColorBrush;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            StopMonitoringAndUnsubscribe();
        }

        public void StopMonitoringAndUnsubscribe()
        {
            // Unsubscribe from events
            _hardwareMonitor.PropertyChanged -= HardwareMonitor_PropertyChanged;
            _loggingService.Log(LogLevel.Info, "System Metrics Dashboard unsubscribed from hardware events");
            // Note: We don't stop the service here, as other parts of the app might use it.
        }
    }
}
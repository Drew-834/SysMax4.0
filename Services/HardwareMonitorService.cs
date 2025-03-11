using System;
using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;
using SysMax2._1.Models; // Added import for LogLevel

namespace SysMax2._1.Services
{
    /// <summary>
    /// Service that provides hardware monitoring using LibreHardwareMonitorLib
    /// </summary>
    public class HardwareMonitorService
    {
        private static HardwareMonitorService? _instance;
        private readonly Computer _computer;
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private bool _isInitialized = false;

        private HardwareMonitorService()
        {
            try
            {
                // Initialize the computer object
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsMotherboardEnabled = true,
                    IsNetworkEnabled = true,
                    IsStorageEnabled = true
                };

                // Open the computer
                _computer.Open();
                _isInitialized = true;

                // Log initialization
                _loggingService.Log(LogLevel.Info, "Hardware Monitor Service initialized successfully");
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error initializing Hardware Monitor Service: {ex.Message}");
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Gets the singleton instance of the HardwareMonitorService
        /// </summary>
        public static HardwareMonitorService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HardwareMonitorService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Updates all hardware information
        /// </summary>
        public void Update()
        {
            if (!_isInitialized)
                return;

            try
            {
                // Update all hardware information
                _computer.Accept(new UpdateVisitor());
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error updating hardware information: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets CPU information
        /// </summary>
        /// <returns>Dictionary with CPU information</returns>
        public Dictionary<string, string> GetCpuInfo()
        {
            Dictionary<string, string> cpuInfo = new Dictionary<string, string>();

            if (!_isInitialized)
                return cpuInfo;

            try
            {
                // Update hardware information
                Update();

                // Find CPU hardware
                var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
                if (cpu != null)
                {
                    // Add CPU name
                    cpuInfo.Add("Name", cpu.Name);

                    // Get CPU sensors
                    foreach (var sensor in cpu.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Package"))
                        {
                            cpuInfo.Add("Temperature", $"{sensor.Value:F1} °C");
                        }
                        else if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core"))
                        {
                            cpuInfo.Add($"Core {sensor.Name.Replace("Core ", "")} Temp", $"{sensor.Value:F1} °C");
                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                        {
                            cpuInfo.Add("Load", $"{sensor.Value:F1} %");
                        }
                        else if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core"))
                        {
                            cpuInfo.Add($"Core {sensor.Name.Replace("Core ", "")} Clock", $"{sensor.Value:F0} MHz");
                        }
                        else if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Package"))
                        {
                            cpuInfo.Add("Power", $"{sensor.Value:F1} W");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting CPU information: {ex.Message}");
            }

            return cpuInfo;
        }

        /// <summary>
        /// Gets GPU information
        /// </summary>
        /// <returns>Dictionary with GPU information</returns>
        public Dictionary<string, string> GetGpuInfo()
        {
            Dictionary<string, string> gpuInfo = new Dictionary<string, string>();

            if (!_isInitialized)
                return gpuInfo;

            try
            {
                // Update hardware information
                Update();

                // Find GPU hardware
                var gpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd);
                if (gpu != null)
                {
                    // Add GPU name
                    gpuInfo.Add("Name", gpu.Name);

                    // Get GPU sensors
                    foreach (var sensor in gpu.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("GPU Core"))
                        {
                            gpuInfo.Add("Temperature", $"{sensor.Value:F1} °C");
                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                        {
                            gpuInfo.Add("Load", $"{sensor.Value:F1} %");
                        }
                        else if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("GPU Core"))
                        {
                            gpuInfo.Add("Core Clock", $"{sensor.Value:F0} MHz");
                        }
                        else if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("GPU Memory"))
                        {
                            gpuInfo.Add("Memory Clock", $"{sensor.Value:F0} MHz");
                        }
                        else if (sensor.SensorType == SensorType.SmallData && sensor.Name.Contains("GPU Memory"))
                        {
                            gpuInfo.Add("Memory Used", $"{sensor.Value:F0} MB");
                        }
                        else if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("GPU Package"))
                        {
                            gpuInfo.Add("Power", $"{sensor.Value:F1} W");
                        }
                        else if (sensor.SensorType == SensorType.Fan && sensor.Name.Contains("GPU"))
                        {
                            gpuInfo.Add("Fan Speed", $"{sensor.Value:F0} RPM");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting GPU information: {ex.Message}");
            }

            return gpuInfo;
        }

        /// <summary>
        /// Gets memory information
        /// </summary>
        /// <returns>Dictionary with memory information</returns>
        public Dictionary<string, string> GetMemoryInfo()
        {
            Dictionary<string, string> memoryInfo = new Dictionary<string, string>();

            if (!_isInitialized)
                return memoryInfo;

            try
            {
                // Update hardware information
                Update();

                // Find memory hardware
                var memory = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
                if (memory != null)
                {
                    // Get memory sensors
                    foreach (var sensor in memory.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Data && sensor.Name.Contains("Memory Used"))
                        {
                            memoryInfo.Add("Used", $"{sensor.Value:F1} GB");
                        }
                        else if (sensor.SensorType == SensorType.Data && sensor.Name.Contains("Memory Available"))
                        {
                            memoryInfo.Add("Available", $"{sensor.Value:F1} GB");
                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Memory"))
                        {
                            memoryInfo.Add("Load", $"{sensor.Value:F1} %");
                        }
                    }

                    // Calculate total memory if we have both used and available
                    if (memoryInfo.ContainsKey("Used") && memoryInfo.ContainsKey("Available"))
                    {
                        float used = float.Parse(memoryInfo["Used"].Replace(" GB", ""));
                        float available = float.Parse(memoryInfo["Available"].Replace(" GB", ""));
                        float total = used + available;
                        memoryInfo.Add("Total", $"{total:F1} GB");
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting memory information: {ex.Message}");
            }

            return memoryInfo;
        }

        /// <summary>
        /// Gets storage information
        /// </summary>
        /// <returns>Dictionary with storage information, where keys are drive names and values are dictionaries with info</returns>
        public Dictionary<string, Dictionary<string, string>> GetStorageInfo()
        {
            Dictionary<string, Dictionary<string, string>> storageInfo = new Dictionary<string, Dictionary<string, string>>();

            if (!_isInitialized)
                return storageInfo;

            try
            {
                // Update hardware information
                Update();

                // Find storage hardware
                var storageDevices = _computer.Hardware.Where(h => h.HardwareType == HardwareType.Storage);
                foreach (var storage in storageDevices)
                {
                    Dictionary<string, string> driveInfo = new Dictionary<string, string>();

                    // Add drive name
                    driveInfo.Add("Name", storage.Name);

                    // Get storage sensors
                    foreach (var sensor in storage.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            driveInfo.Add("Temperature", $"{sensor.Value:F1} °C");
                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Used Space"))
                        {
                            driveInfo.Add("Used Space", $"{sensor.Value:F1} %");
                        }
                        else if (sensor.SensorType == SensorType.Data && sensor.Name.Contains("Read Rate"))
                        {
                            driveInfo.Add("Read Rate", $"{sensor.Value:F1} MB/s");
                        }
                        else if (sensor.SensorType == SensorType.Data && sensor.Name.Contains("Write Rate"))
                        {
                            driveInfo.Add("Write Rate", $"{sensor.Value:F1} MB/s");
                        }
                    }

                    // Add drive info to storage info
                    storageInfo.Add(storage.Name, driveInfo);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting storage information: {ex.Message}");
            }

            return storageInfo;
        }

        /// <summary>
        /// Gets network information
        /// </summary>
        /// <returns>Dictionary with network information, where keys are adapter names and values are dictionaries with info</returns>
        public Dictionary<string, Dictionary<string, string>> GetNetworkInfo()
        {
            Dictionary<string, Dictionary<string, string>> networkInfo = new Dictionary<string, Dictionary<string, string>>();

            if (!_isInitialized)
                return networkInfo;

            try
            {
                // Update hardware information
                Update();

                // Find network hardware
                var networkAdapters = _computer.Hardware.Where(h => h.HardwareType == HardwareType.Network);
                foreach (var network in networkAdapters)
                {
                    Dictionary<string, string> adapterInfo = new Dictionary<string, string>();

                    // Add adapter name
                    adapterInfo.Add("Name", network.Name);

                    // Get network sensors
                    foreach (var sensor in network.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Throughput && sensor.Name.Contains("Download"))
                        {
                            adapterInfo.Add("Download", $"{sensor.Value / 1024:F1} KB/s");
                        }
                        else if (sensor.SensorType == SensorType.Throughput && sensor.Name.Contains("Upload"))
                        {
                            adapterInfo.Add("Upload", $"{sensor.Value / 1024:F1} KB/s");
                        }
                        else if (sensor.SensorType == SensorType.Data && sensor.Name.Contains("Data Downloaded"))
                        {
                            adapterInfo.Add("Data Downloaded", $"{sensor.Value:F1} MB");
                        }
                        else if (sensor.SensorType == SensorType.Data && sensor.Name.Contains("Data Uploaded"))
                        {
                            adapterInfo.Add("Data Uploaded", $"{sensor.Value:F1} MB");
                        }
                    }

                    // Only add if we have download or upload info
                    if (adapterInfo.ContainsKey("Download") || adapterInfo.ContainsKey("Upload"))
                    {
                        networkInfo.Add(network.Name, adapterInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting network information: {ex.Message}");
            }

            return networkInfo;
        }

        /// <summary>
        /// Gets motherboard information
        /// </summary>
        /// <returns>Dictionary with motherboard information</returns>
        public Dictionary<string, string> GetMotherboardInfo()
        {
            Dictionary<string, string> motherboardInfo = new Dictionary<string, string>();

            if (!_isInitialized)
                return motherboardInfo;

            try
            {
                // Update hardware information
                Update();

                // Find motherboard hardware
                var motherboard = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Motherboard);
                if (motherboard != null)
                {
                    // Add motherboard name
                    motherboardInfo.Add("Name", motherboard.Name);

                    // Get motherboard sensors
                    foreach (var sensor in motherboard.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            motherboardInfo.Add(sensor.Name, $"{sensor.Value:F1} °C");
                        }
                        else if (sensor.SensorType == SensorType.Fan)
                        {
                            motherboardInfo.Add(sensor.Name, $"{sensor.Value:F0} RPM");
                        }
                        else if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("Voltage"))
                        {
                            motherboardInfo.Add(sensor.Name, $"{sensor.Value:F2} V");
                        }
                    }

                    // Get super IO information
                    foreach (var subHardware in motherboard.SubHardware)
                    {
                        foreach (var sensor in subHardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                motherboardInfo.Add(sensor.Name, $"{sensor.Value:F1} °C");
                            }
                            else if (sensor.SensorType == SensorType.Fan)
                            {
                                motherboardInfo.Add(sensor.Name, $"{sensor.Value:F0} RPM");
                            }
                            else if (sensor.SensorType == SensorType.Voltage)
                            {
                                motherboardInfo.Add(sensor.Name, $"{sensor.Value:F2} V");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting motherboard information: {ex.Message}");
            }

            return motherboardInfo;
        }

        /// <summary>
        /// Close and dispose of the computer object
        /// </summary>
        public void Close()
        {
            if (_isInitialized)
            {
                try
                {
                    _computer.Close();
                    _loggingService.Log(LogLevel.Info, "Hardware Monitor Service closed");
                }
                catch (Exception ex)
                {
                    _loggingService.Log(LogLevel.Error, $"Error closing Hardware Monitor Service: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Visitor class for updating hardware information
    /// </summary>
    internal class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();

            foreach (var subHardware in hardware.SubHardware)
            {
                subHardware.Accept(this);
            }
        }

        public void VisitSensor(ISensor sensor) { }

        public void VisitParameter(IParameter parameter) { }
    }
}
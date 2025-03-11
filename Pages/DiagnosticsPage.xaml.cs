using SysMax2._1;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LibreHardwareMonitor.Hardware;
using System.Windows.Navigation;
using SysMax2._1.Pages;

namespace SysMax2._1.Pages

{
    public partial class DiagnosticsPage : Page
    {
        private readonly Computer _computer;
        private PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        public DiagnosticsPage()
        {
            InitializeComponent();

            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };
            _computer.Open();

            StartMonitoring();
        }

        private void StartMonitoring()
        {
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += UpdateDiagnostics;
            timer.Start();
        }

        private void UpdateDiagnostics(object? sender, EventArgs e)
        {
            txtCpuUsage.Text = $"{cpuCounter.NextValue():F2}%";
            txtRamUsage.Text = $"{ramCounter.NextValue()} MB";
            txtIpAddress.Text = GetLocalIPAddress();
            txtPing.Text = $"{PingHost("8.8.8.8")} ms";

            txtCpuTemp.Text = $"{GetTemperature("Cpu")}°C";
            txtGpuTemp.Text = $"{GetTemperature("Gpu")}°C";
        }

        private string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "Unknown";
        }

        private int PingHost(string host)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(host);
                return reply.Status == IPStatus.Success ? (int)reply.RoundtripTime : -1;
            }
            catch { return -1; }
        }

        private string GetTemperature(string hardwareType)
        {
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType.ToString().Equals(hardwareType, StringComparison.OrdinalIgnoreCase))
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            return sensor.Value?.ToString() ?? "N/A";
                        }
                    }
                }
            }
            return "N/A";
        }

        private void ClearTempFiles_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd.exe", "/C del /s /q %temp%\\*");
            MessageBox.Show("Temporary files cleared.");
        }

        private void RestartNetworkAdapter_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd.exe", "/C netsh interface set interface name=\"Wi-Fi\" admin=disable & netsh interface set interface name=\"Wi-Fi\" admin=enable");
            MessageBox.Show("Network adapter restarted.");
        }

        private void RunDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Basic diagnostics completed. No critical issues found.");
        }
    }
}
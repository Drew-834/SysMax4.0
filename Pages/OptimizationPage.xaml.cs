using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Management;

namespace SysMax2._1.Pages
{
    public partial class OptimizationPage : Page
    {
        public OptimizationPage()
        {
            InitializeComponent();
            LoadSystemInfo();
        }

        private void LoadSystemInfo()
        {
            var cpuUsage = GetCpuUsage();
            CpuUsageBar.Value = double.IsFinite(cpuUsage) ? cpuUsage : 0;
            CpuUsageText.Text = $"{CpuUsageBar.Value}%";

            var ramUsage = GetRamUsage();
            RamUsageBar.Value = double.IsFinite(ramUsage) ? ramUsage : 0;
            RamUsageText.Text = $"{RamUsageBar.Value}%";

            LoadStartupApplications();
            LoadRunningProcesses();
        }

        private double GetCpuUsage()
        {
            try
            {
                using (PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    cpuCounter.NextValue(); // First value is usually zero, so discard it
                    System.Threading.Thread.Sleep(1000);
                    double value = cpuCounter.NextValue();

                    if (!double.IsFinite(value) || value < 0)
                        return 0; // Avoid NaN, -∞, or +∞ values

                    return Math.Round(value, 2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving CPU usage: " + ex.Message);
                return 0;
            }
        }

        private double GetRamUsage()
        {
            try
            {
                using (PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes"))
                {
                    ulong totalRam = GetTotalPhysicalMemory();
                    if (totalRam == 0) return 0; // Avoid division by zero

                    double usedRam = totalRam - (ramCounter.NextValue() * 1024 * 1024);
                    double usage = (usedRam / totalRam) * 100;

                    if (!double.IsFinite(usage) || usage < 0 || usage > 100)
                        return 0; // Ensure valid percentage

                    return Math.Round(usage, 2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving RAM usage: " + ex.Message);
                return 0;
            }
        }

        private ulong GetTotalPhysicalMemory()
        {
            ulong totalRam = 0;
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalRam = (ulong)obj["TotalPhysicalMemory"];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving total physical memory: " + ex.Message);
            }
            return totalRam;
        }

        private void LoadStartupApplications()
        {
            StartupList.Items.Clear();
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    foreach (string appName in key.GetValueNames())
                    {
                        StartupList.Items.Add(appName);
                    }
                }
            }
        }

        private void LoadRunningProcesses()
        {
            ProcessList.Items.Clear();
            var processes = Process.GetProcesses()
                                   .Select(p => p.ProcessName)
                                   .Distinct()
                                   .OrderBy(p => p);

            foreach (var process in processes)
            {
                ProcessList.Items.Add(process);
            }
        }

        private void DisableStartup_Click(object sender, RoutedEventArgs e)
        {
            if (StartupList.SelectedItem == null) return;

            string selectedApp = StartupList.SelectedItem.ToString();
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(selectedApp, false);
                    }
                }

                LoadStartupApplications(); // Refresh after disabling
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disabling startup program: {ex.Message}");
            }
        }

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem == null) return;

            string selectedProcess = ProcessList.SelectedItem.ToString();
            try
            {
                foreach (var process in Process.GetProcessesByName(selectedProcess))
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error terminating process: {ex.Message}");
            }

            LoadRunningProcesses(); // Refresh after killing process
        }

        private void CheckPing_Click(object sender, RoutedEventArgs e)
        {
            string host = PingInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(host)) return;

            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(host, 1000);
                    PingResultText.Text = reply.Status == IPStatus.Success
                        ? $"Ping: {reply.RoundtripTime} ms"
                        : "Ping failed.";
                }
            }
            catch (Exception ex)
            {
                PingResultText.Text = $"Error: {ex.Message}";
            }
        }
    }
}

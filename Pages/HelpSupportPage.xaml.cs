using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using SysMax2._1.Models;
using SysMax2._1.Services;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for HelpSupportPage.xaml
    /// </summary>
    public partial class HelpSupportPage : Page
    {
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private MainWindow? mainWindow;
        private string videosFolderPath;

        public HelpSupportPage()
        {
            InitializeComponent();

            // Find main window
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Set videos folder path
            videosFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Videos");
            
            // Log page navigation
            _loggingService.Log(LogLevel.Info, "Navigated to Help & Support page");

            // Update status
            if (mainWindow != null)
            {
                mainWindow.UpdateStatus("Viewing Help & Support");
                mainWindow.ShowAssistantMessage("Need help with SysMax? This page provides FAQs, tutorials, and contact information for our support team.");
            }
        }

        private void SendFeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // In a real application, this would open a feedback form
                // For now, we'll simulate with a message

                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("Thank you for wanting to provide feedback! In the full version, this would open a feedback form where you could share your thoughts and suggestions.");
                }

                // Log the action
                _loggingService.Log(LogLevel.Info, "User clicked Send Feedback button");

                // Show a simple feedback dialog
                MessageBox.Show(
                    "Thank you for wanting to provide feedback!\n\nIn the full version of SysMax, this would open a feedback form where you could share your experience, report issues, or suggest improvements.",
                    "Send Feedback",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error showing feedback dialog: {ex.Message}");
            }
        }

        private void IntroVideoArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string introVideoPath = Path.Combine(videosFolderPath, "copy_DD25E089-4F02-414F-A4B6-1E1DA114E003 (1).mov");
                
                if (!File.Exists(introVideoPath))
                {
                    throw new FileNotFoundException($"Video file not found at path: {introVideoPath}");
                }
                
                // Log the action
                _loggingService.Log(LogLevel.Info, "User clicked Intro Video tutorial");
                
                // Show message before playing
                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("Opening the Intro Video in your system's default media player...");
                }
                
                // Open video in the default system player
                var processInfo = new ProcessStartInfo
                {
                    FileName = introVideoPath,
                    UseShellExecute = true
                };
                
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error playing Intro video: {ex.Message}");
                MessageBox.Show($"Could not play video: {ex.Message}", "Video Playback Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ITSolutionVideoArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string itSolutionVideoPath = Path.Combine(videosFolderPath, "IT Solution.mov");
                
                if (!File.Exists(itSolutionVideoPath))
                {
                    throw new FileNotFoundException($"Video file not found at path: {itSolutionVideoPath}");
                }
                
                // Log the action
                _loggingService.Log(LogLevel.Info, "User clicked IT Solution Video tutorial");
                
                // Show message before playing
                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("Opening the IT Solution Video in your system's default media player...");
                }
                
                // Open video in the default system player
                var processInfo = new ProcessStartInfo
                {
                    FileName = itSolutionVideoPath,
                    UseShellExecute = true
                };
                
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error playing IT Solution video: {ex.Message}");
                MessageBox.Show($"Could not play video: {ex.Message}", "Video Playback Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
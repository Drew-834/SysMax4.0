using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public HelpSupportPage()
        {
            InitializeComponent();

            // Find main window
            mainWindow = Window.GetWindow(this) as MainWindow;

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

        private void VideoThumbnail_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // In a real application, this would play a video tutorial
                // For now, we'll simulate with a message

                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("In the full version, this would play a video tutorial to help you learn how to use SysMax effectively.");
                }

                // Get the title of the video
                if (sender is FrameworkElement element)
                {
                    // Find the parent Border
                    if (element.Parent is FrameworkElement parent)
                    {
                        // Find the Grid
                        if (parent.Parent is Grid grid)
                        {
                            // Find the StackPanel
                            StackPanel? stackPanel = null;
                            foreach (var child in grid.Children)
                            {
                                if (child is StackPanel && Grid.GetRow((FrameworkElement)child) == 1)
                                {
                                    stackPanel = child as StackPanel;
                                    break;
                                }
                            }

                            if (stackPanel != null && stackPanel.Children.Count > 0 && stackPanel.Children[0] is TextBlock titleBlock)
                            {
                                string videoTitle = titleBlock.Text;

                                // Log the action
                                _loggingService.Log(LogLevel.Info, $"User clicked video tutorial: {videoTitle}");

                                // Show a simple message dialog
                                MessageBox.Show(
                                    $"You clicked on the '{videoTitle}' tutorial.\n\nIn the full version of SysMax, this would play a video tutorial to help you learn how to use this feature.",
                                    "Video Tutorial",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                return;
                            }
                        }
                    }
                }

                // Fallback if we can't determine the video title
                _loggingService.Log(LogLevel.Info, "User clicked video tutorial");

                MessageBox.Show(
                    "In the full version of SysMax, this would play a video tutorial to help you learn how to use SysMax effectively.",
                    "Video Tutorial",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error showing video dialog: {ex.Message}");
            }
        }
    }
}
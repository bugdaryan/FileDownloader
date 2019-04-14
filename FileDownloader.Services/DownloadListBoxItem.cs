using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using ProgressBar = System.Windows.Controls.ProgressBar;
using TextBox = System.Windows.Controls.TextBox;


namespace FileDownloader.Services
{
    public class DownloadListBoxItem
    {
        public Button DownloadButton { get; set; }
        public Button CancelButton { get; set; }
        public Button RemoveButton { get; set; }
        public TextBox DownloadTextBox { get; set; }
        public ProgressBar DownloadProgressBar { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public Border DownloadBorder { get; set; }
        public StackPanel DownloadPanel { get; set; }
        public Label ProgressLabel { get; set; }

        public DownloadListBoxItem(int width)
        {
            DownloadBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2)
            };

            DownloadPanel = new StackPanel
            {
                Width = width * .95,
                Height = 100,
            };

            DownloadTextBox = new TextBox
            {
                Width = width * 0.9,
                Height = 30,
                Margin = new Thickness(5, 10, 0, 0),
            };

            DownloadProgressBar = new ProgressBar
            {
                Width = DownloadTextBox.Width * .9,
                Height = 30,
                Margin = new Thickness(10, 10, 10, 0),
                Visibility = Visibility.Collapsed
            };

            ProgressLabel = new Label
            {
                Content = "0%",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0),
                Visibility = Visibility.Collapsed
            };

            RemoveButton = new Button
            {
                Margin = new Thickness(10, 10, 10, 10),
                Width = 100,
                Height = 30,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Content = "Remove",
                Background = Brushes.Red,
                Foreground = Brushes.White,
            };

            CancelButton = new Button
            {
                Margin = new Thickness(10, 0, 10, 10),
                Width = 100,
                Height = 30,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Content = "Cancel",
                Background = Brushes.Gray,
                Foreground = Brushes.White,
                Visibility = Visibility.Collapsed
            };

            DownloadButton = new Button
            {
                Margin = new Thickness(10, 10, 10, 10),
                Width = 100,
                Height = 30,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Content = "Download",
                Background = Brushes.DodgerBlue,
                Foreground = Brushes.White,
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(DownloadProgressBar);
            stackPanel.Children.Add(ProgressLabel);

            var grid = new Grid();
            grid.Children.Add(DownloadButton);
            grid.Children.Add(RemoveButton);

            DownloadBorder.Child = DownloadPanel;

            DownloadPanel.Children.Add(DownloadTextBox);
            DownloadPanel.Children.Add(grid);
            DownloadPanel.Children.Add(stackPanel);
            DownloadPanel.Children.Add(CancelButton);
        }

        private void ShowDownloadStuff()
        {
            CancelButton.Visibility = Visibility.Collapsed;
            DownloadProgressBar.Visibility = Visibility.Collapsed;
            ProgressLabel.Visibility = Visibility.Collapsed;
            DownloadButton.Visibility = Visibility.Visible;
            DownloadTextBox.Visibility = Visibility.Visible;
            RemoveButton.Visibility = Visibility.Visible;
        }

        private void HideDownloadStuff()
        {
            CancelButton.Visibility = Visibility.Visible;
            DownloadProgressBar.Visibility = Visibility.Visible;
            ProgressLabel.Visibility = Visibility.Visible;
            DownloadButton.Visibility = Visibility.Collapsed;
            DownloadTextBox.Visibility = Visibility.Collapsed;
            RemoveButton.Visibility = Visibility.Collapsed;
        }

        public async Task DownloadAsync()
        {
            DownloadProgressBar.Value = 0;
            ProgressLabel.Content = "0%";
            using (var dlg = new System.Windows.Forms.SaveFileDialog())
            {
                var dialogResult = dlg.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    var filePath = dlg.FileName;
                    if (DownloadTextBox.Text != string.Empty &&
                        Uri.IsWellFormedUriString(DownloadTextBox.Text, UriKind.RelativeOrAbsolute))
                    {
                        using (var client = new HttpClientWithProgress(DownloadTextBox.Text, filePath))
                        {
                            TokenSource = new CancellationTokenSource();

                            TokenSource.Token.Register(() =>
                            {
                                client.CancelPendingRequests();

                                ShowDownloadStuff();
                            });

                            HideDownloadStuff();

                            try
                            {
                                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                                {
                                    if (progressPercentage.HasValue)
                                    {
                                        DownloadProgressBar.Value = progressPercentage.Value;
                                        ProgressLabel.Content = $"{(int)progressPercentage.Value}%";
                                    }
                                };
                                await client.StartDownloadAsync();
                            }
                            catch (HttpRequestException exception)
                            {
                                MessageBox.Show(exception.Message);
                            }
                            catch (TaskCanceledException)
                            {
                                MessageBox.Show("Downloading was canceled");
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                ShowDownloadStuff();

                                TokenSource.Dispose();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid uri, please enter valid one");
                    }
                }
            }
        }
    }
}


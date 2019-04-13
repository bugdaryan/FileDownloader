using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using ProgressBar = System.Windows.Controls.ProgressBar;
using TextBox = System.Windows.Controls.TextBox;
namespace FileDownloader.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<Button, (TextBox, ProgressBar, Button)> downloadControls;
        int index = 0;
        CancellationToken cancelToken;
        CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
            downloadControls = new Dictionary<Button, (TextBox, ProgressBar, Button)>();

            cts = new CancellationTokenSource();
            cancelToken = new CancellationToken();
            cancelToken = cts.Token;
        }

        private async Task DownloadAsync(Button button)
        {
            if (downloadControls.ContainsKey(button))
            {
                var textBox = downloadControls[button].Item1;

                var progressBar = downloadControls[button].Item2;

                var cancelButton = downloadControls[button].Item3;

                int ind = textBox.Name[textBox.Name.Length - 1] - '0';
                progressBar.Value = 0;

                using (var dlg = new SaveFileDialog())
                {
                    var dialogResult = dlg.ShowDialog();
                    if (dialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        var filePath = dlg.FileName;
                        if (textBox.Text != string.Empty && Uri.IsWellFormedUriString(textBox.Text, UriKind.RelativeOrAbsolute))
                        {
                            using (var client = new WebClient())
                            {
                                client.DownloadProgressChanged += (send, args) =>
                                {
                                    progressBar.Value = args.ProgressPercentage;
                                };
                                cts = new CancellationTokenSource();

                                cts.Token.Register(() =>
                                {
                                    cancelButton.Visibility = Visibility.Collapsed;
                                    progressBar.Visibility = Visibility.Collapsed;
                                    button.Visibility = Visibility.Visible;
                                    textBox.Visibility = Visibility.Visible;

                                    client.CancelAsync();
                                });

                                cancelButton.Visibility = Visibility.Visible;
                                progressBar.Visibility = Visibility.Visible;
                                button.Visibility = Visibility.Collapsed;
                                textBox.Visibility = Visibility.Collapsed;

                                try
                                {
                                    await client.DownloadFileTaskAsync(textBox.Text, filePath);

                                    MessageBox.Show($"Download {ind} completed!");
                                }
                                catch (WebException exception)
                                {
                                    MessageBox.Show(exception.Message);
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                                finally
                                {
                                    cancelButton.Visibility = Visibility.Collapsed;
                                    progressBar.Visibility = Visibility.Collapsed;
                                    button.Visibility = Visibility.Visible;
                                    textBox.Visibility = Visibility.Visible;

                                    cts.Dispose();
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

        private void AddNewDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2)
            };

            var newDownload = new StackPanel
            {
                Width = DownloadsListBox.Width * .95,
                Height = 70,
                Name = $"DownloadStackPanel_{index}"
            };
            border.Child = newDownload;

            var textBox = new System.Windows.Controls.TextBox
            {
                Width = DownloadsListBox.Width * 0.9,
                Height = 30,
                Margin = new Thickness(5, 0, 0, 0),
                Name = $"DownloadTextbox_{index}"
            };
            newDownload.Children.Add(textBox);

            var cancelButton = new Button
            {
                Margin = new Thickness(10, 10, 10, 0),
                Width = 100,
                Height = 30,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Content = "Cancel",
                Background = Brushes.DodgerBlue,
                Foreground = Brushes.White,
                Name = $"CancelButton_{index}",
                Visibility = Visibility.Collapsed
            };
            cancelButton.Click += CancelButton_Click;
            newDownload.Children.Add(cancelButton);

            var button = new Button
            {
                Margin = new Thickness(10, 10, 10, 10),
                Width = 100,
                Height = 30,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Content = "Download",
                Background = Brushes.DodgerBlue,
                Foreground = Brushes.White,
                Name = $"DownloadButton_{index}"
            };
            button.Click += Download_ClickAsync;
            newDownload.Children.Add(button);

            var progressBar = new System.Windows.Controls.ProgressBar
            {
                Name = $"DownloadProgressBar_{index}",
                Width = 200,
                Height = 30,
                Margin = new Thickness(10)
            };
            progressBar.Visibility = Visibility.Collapsed;
            newDownload.Children.Add(progressBar);

            DownloadsListBox.Items.Add(border);
            downloadControls.Add(button, (textBox, progressBar, cancelButton));
            index++;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            cts.Dispose();
        }

        private async void Download_ClickAsync(object sender, RoutedEventArgs e)
        {
            var button = ((Button)sender);
            await DownloadAsync(button);
        }
    }
}


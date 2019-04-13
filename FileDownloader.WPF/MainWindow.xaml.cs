using System;
using System.Collections.Generic;
using System.IO;
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
        Dictionary<Button, CancellationTokenSource> tokens;

        public MainWindow()
        {
            InitializeComponent();
            downloadControls = new Dictionary<Button, (TextBox, ProgressBar, Button)>();

            tokens = new Dictionary<Button, CancellationTokenSource>();
        }

        private async Task DownloadAsync(Button button)
        {
            if (downloadControls.ContainsKey(button))
            {
                var textBox = downloadControls[button].Item1;
                var progressBar = downloadControls[button].Item2;
                var cancelButton = downloadControls[button].Item3;

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
                                var token = new CancellationTokenSource();
                                tokens[cancelButton] = token;
                                token.Token.Register(() =>
                                {
                                    cancelButton.Visibility = Visibility.Collapsed;
                                    progressBar.Visibility = Visibility.Collapsed;
                                    button.Visibility = Visibility.Visible;
                                    textBox.Visibility = Visibility.Visible;

                                    client.CancelAsync();
                                    client.Dispose();

                                    ////////////////////////
                                    //GC.Collect();
                                    //GC.WaitForPendingFinalizers();
                                    ///////////////////////

                                    var file = new FileInfo(filePath);
                                    file.Attributes = file.Attributes & ~FileAttributes.ReadOnly;

                                    try
                                    {
                                        file.Delete();
                                    }
                                    catch (IOException ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                           
                                    //File.Delete(filePath);
                                });

                                cancelButton.Visibility = Visibility.Visible;
                                progressBar.Visibility = Visibility.Visible;
                                button.Visibility = Visibility.Collapsed;
                                textBox.Visibility = Visibility.Collapsed;

                                try
                                {
                                    await client.DownloadFileTaskAsync(textBox.Text, filePath);
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

                                    token.Dispose();
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
                Height = 90,
            };
            border.Child = newDownload;

            var textBox = new System.Windows.Controls.TextBox
            {
                Width = DownloadsListBox.Width * 0.9,
                Height = 30,
                Margin = new Thickness(5, 10, 0, 0),
            };
            newDownload.Children.Add(textBox);

            var progressBar = new System.Windows.Controls.ProgressBar
            {
                Width = textBox.Width * .9,
                Height = 30,
                Margin = new Thickness(10)
            };
            progressBar.Visibility = Visibility.Collapsed;
            newDownload.Children.Add(progressBar);

            var cancelButton = new Button
            {
                Margin = new Thickness(10, 10, 10, 10),
                Width = 100,
                Height = 30,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Content = "Cancel",
                Background = Brushes.DodgerBlue,
                Foreground = Brushes.White,
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
            };
            button.Click += Download_ClickAsync;
            newDownload.Children.Add(button);

            DownloadsListBox.Items.Add(border);
            downloadControls.Add(button, (textBox, progressBar, cancelButton));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var cancelButton = (Button)sender;
            tokens[cancelButton].Cancel();
            tokens[cancelButton].Dispose();
        }

        private async void Download_ClickAsync(object sender, RoutedEventArgs e)
        {
            var button = ((Button)sender);
            await DownloadAsync(button);
        }
    }
}


using FileDownloader.Services;
using System.Collections.Generic;
using System.Windows;
using Button = System.Windows.Controls.Button;
namespace FileDownloader.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<Button, DownloadListBoxItem> downloadControls;
        Dictionary<Button, Button> buttons;

        public MainWindow()
        {
            InitializeComponent();
            downloadControls = new Dictionary<Button, DownloadListBoxItem>();

            buttons = new Dictionary<Button, Button>();
        }

        private void AddNewDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadListBoxItem item = new DownloadListBoxItem((int)DownloadsListBox.Width);
            item.CancelButton.Click += CancelButton_Click;
            item.DownloadButton.Click += Download_ClickAsync;

            DownloadsListBox.Items.Add(item.DownloadBorder);

            downloadControls.Add(item.DownloadButton, item);
            buttons.Add(item.CancelButton, item.DownloadButton);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var cancelButton = (Button)sender;
            var downloadButton = buttons[cancelButton];
            downloadControls[downloadButton].TokenSource.Cancel();
            downloadControls[downloadButton].TokenSource.Dispose();
        }

        private async void Download_ClickAsync(object sender, RoutedEventArgs e)
        {
            var downloadButton = ((Button)sender);

            await downloadControls[downloadButton].DownloadAsync();
        }
    }
}


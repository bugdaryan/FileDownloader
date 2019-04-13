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
        Dictionary<Button, Button> cancelAddButtons;
        Dictionary<Button, Button> removeAddButtons; 

        public MainWindow()
        {
            InitializeComponent();
            downloadControls = new Dictionary<Button, DownloadListBoxItem>();

             cancelAddButtons = new Dictionary<Button, Button>();
            removeAddButtons = new Dictionary<Button, Button>();
        }

        private void AddNewDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadListBoxItem item = new DownloadListBoxItem((int)DownloadsListBox.Width);
            item.CancelButton.Click += CancelButton_Click;
            item.DownloadButton.Click += Download_ClickAsync;
            item.RemoveButton.Click += RemoveButton_Click;

            DownloadsListBox.Items.Add(item.DownloadBorder);

            downloadControls.Add(item.DownloadButton, item);
            cancelAddButtons.Add(item.CancelButton, item.DownloadButton);
            removeAddButtons.Add(item.RemoveButton, item.DownloadButton);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var removeButton = (Button)sender;
            var downloadButton = removeAddButtons[removeButton];
            var item = downloadControls[downloadButton];

            removeAddButtons.Remove(removeButton);
            cancelAddButtons.Remove(item.CancelButton);
            downloadControls.Remove(downloadButton);

            DownloadsListBox.Items.Remove(item.DownloadBorder);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var cancelButton = (Button)sender;
            var downloadButton = cancelAddButtons[cancelButton];
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


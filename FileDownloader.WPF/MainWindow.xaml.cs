using FileDownloader.Services;
using System.Windows;
using Button = System.Windows.Controls.Button;
namespace FileDownloader.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DownloadItemsCollection items;

        public MainWindow()
        {
            InitializeComponent();
            items = new DownloadItemsCollection();
        }

        private void AddNewDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadListBoxItem item = new DownloadListBoxItem((int)(DownloadsListBox.Width*.95));
            item.CancelButton.Click += CancelButton_Click;
            item.DownloadButton.Click += Download_ClickAsync;
            item.RemoveButton.Click += RemoveButton_Click;

            DownloadsListBox.Items.Add(item.DownloadBorder);

            items.Add(item);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var removeButton = (Button)sender;
            DownloadsListBox.Items.Remove(items.GetByRemoveButton(removeButton).DownloadBorder);
            items.Remove(removeButton);

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var cancelButton = (Button)sender;
            var item = items.GetByCancelButton(cancelButton);
            item.TokenSource.Cancel();
            item.TokenSource.Dispose();
        }

        private async void Download_ClickAsync(object sender, RoutedEventArgs e)
        {
            var downloadButton = ((Button)sender);

            await items.GetByDownloadButton(downloadButton).DownloadAsync();
        }
    }
}


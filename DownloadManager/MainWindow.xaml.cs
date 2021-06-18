using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AltoHttp;
using AltoHttp.Exceptions;

namespace DownloadManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpDownloader httpDownloader;
        string DownloadFolderPath = "";

        public MainWindow()
        {
            InitializeComponent();
            if(httpDownloader == null)
            {
                BtnPause.IsEnabled = false;
                BtnResume.IsEnabled = false;
            }
            CheckForDownloadPath();
        }

        private void CheckForDownloadPath()
        {
            // If Download folder doesnt exist Create it
            string DownloaderFolderName = "Downloads";
            string applicationDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            string downloadFolderPath = applicationDirectory + "/" + DownloaderFolderName;
            bool exists = Directory.Exists(downloadFolderPath);

            if (!exists)
            {
                Directory.CreateDirectory(DownloaderFolderName);
            }

            //assign DownloadFolderPath
            DownloadFolderPath = downloadFolderPath;
        }


        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtURL.Text))
            {
                System.Windows.MessageBox.Show("Please Enter A URL.", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var defaultFileName = "default.unknown";
                var defaultSavePath = Path.Combine(DownloadFolderPath, defaultFileName);
                httpDownloader = new HttpDownloader(TxtURL.Text, defaultSavePath);
                httpDownloader.DownloadCompleted += HttpDownloader_DownloadCompleted;
                httpDownloader.ProgressChanged += HttpDownloader_ProgressChanged;
                httpDownloader.DownloadInfoReceived += HttpDownloader_DownloadInfoReceived;
                httpDownloader.StatusChanged += HttpDownloader_StatusChanged;
                httpDownloader.ErrorOccured += HttpDownloader_ErrorOccured;
                httpDownloader.Start();

            }
        }

        private void HttpDownloader_ErrorOccured(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            if (ex is FileValidationFailedException)
            {
                httpDownloader.Pause();
            }
            System.Windows.MessageBox.Show("Error: " + e.GetException().Message + " " + e.GetException().StackTrace);
        }

        private void HttpDownloader_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.Status == Status.Downloading)
            {
                BtnResume.IsEnabled = false;
                BtnPause.IsEnabled = true;
                BtnDownload.IsEnabled = false;

                TxtStatus.Text = "Status : Downloading...";
            }
            else if (e.Status == Status.Paused)
            {
                BtnResume.IsEnabled = true;
                BtnPause.IsEnabled = false;
                BtnDownload.IsEnabled = false;

                TxtStatus.Text = "Status : Paused!";
            }
        }

        private void HttpDownloader_DownloadInfoReceived(object sender, EventArgs e)
        {
            var saveDirectory = Path.GetDirectoryName(httpDownloader.FullFileName);
            var serverFileName = httpDownloader.Info.ServerFileName;

            var newFilePath = Path.Combine(saveDirectory, serverFileName);
            httpDownloader.FullFileName = newFilePath;
        }

        private void HttpDownloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PBDownload.Value = (int)e.Progress;
            TxtPercent.Text = $"Percentage : {e.Progress.ToString("0.00")}%";
            TxtSpeed.Text = e.SpeedInBytes.ToHumanReadableSize() + "/s";
            TxtDownloadSize.Text = string.Format("{0} / {1}",
                e.TotalBytesReceived.ToHumanReadableSize(),
                httpDownloader.Info.Length > 0 ? httpDownloader.Info.Length.ToHumanReadableSize() : "Unknown");
        }

        private void HttpDownloader_DownloadCompleted(object sender, System.EventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                TxtStatus.Text = "Status : Finished!";
                TxtPercent.Text = "Percentage : 100%";
            }));
        }

        private void BtnResume_Click(object sender, RoutedEventArgs e)
        {
            if (httpDownloader != null)
                httpDownloader.Resume();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if(httpDownloader!= null)
                httpDownloader.Pause();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}

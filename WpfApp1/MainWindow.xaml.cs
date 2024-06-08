using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms; // Потрібне посилання на System.Windows.Forms
using System.Collections.Generic;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private long totalBytes;
        private long copiedBytes;
        private int numThreads;
        private ManualResetEvent pauseEvent = new ManualResetEvent(true);
        private bool stopCopying;
        private Thread[] copyThreads;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    SourceTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    DestinationTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void StartCopy_Click(object sender, RoutedEventArgs e)
        {
            stopCopying = false;
            pauseEvent.Set();

            string sourceDir = SourceTextBox.Text;
            string destinationDir = DestinationTextBox.Text;
            numThreads = int.Parse(ThreadsTextBox.Text);

            totalBytes = CalculateTotalBytes(sourceDir);
            copiedBytes = 0;

            copyThreads = new Thread[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                copyThreads[i] = new Thread(() => CopyDirectory(sourceDir, destinationDir));
                copyThreads[i].Start();
            }
        }

        private long CalculateTotalBytes(string dir)
        {
            long totalSize = 0;
            foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
            {
                totalSize += new FileInfo(file).Length;
            }
            return totalSize;
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            try
            {
                foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourceDir, destinationDir));
                }

                foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    if (stopCopying)
                    {
                        return;
                    }

                    pauseEvent.WaitOne();

                    string destFilePath = filePath.Replace(sourceDir, destinationDir);
                    CopyFile(filePath, destFilePath);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => System.Windows.MessageBox.Show($"Error: {ex.Message}"));
            }
        }

        private void CopyFile(string sourceFilePath, string destFilePath)
        {
            byte[] buffer = new byte[1024 * 1024];
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream destStream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write))
                {
                    int bytesRead;
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (stopCopying)
                        {
                            return;
                        }

                        pauseEvent.WaitOne();

                        destStream.Write(buffer, 0, bytesRead);
                        Interlocked.Add(ref copiedBytes, bytesRead);

                        double progress = ((double)copiedBytes / totalBytes) * 100;
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar.Value = progress;
                            ProgressTextBlock.Text = $"Progress: {progress:F2}%";
                        });
                    }
                }
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private ManualResetEvent pauseEvent = new ManualResetEvent(true);
        private bool stopCopying;
        private Thread copyThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SourceTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                DestinationTextBox.Text = saveFileDialog.FileName;
            }
        }

        private void StartCopy_Click(object sender, RoutedEventArgs e)
        {
            stopCopying = false;
            pauseEvent.Set(); // У разі повторного старту
            string sourcePath = SourceTextBox.Text;
            string destinationPath = DestinationTextBox.Text;
            int numThreads = int.Parse(ThreadsTextBox.Text);

            copyThread = new Thread(() => CopyFile(sourcePath, destinationPath, numThreads));
            copyThread.Start();
        }

        private void PauseCopy_Click(object sender, RoutedEventArgs e)
        {
            if (pauseEvent.WaitOne(0))
            {
                pauseEvent.Reset();
                Dispatcher.Invoke(() => { ((Button)sender).Content = "Resume"; });
            }
            else
            {
                pauseEvent.Set();
                Dispatcher.Invoke(() => { ((Button)sender).Content = "Pause"; });
            }
        }

        private void StopCopy_Click(object sender, RoutedEventArgs e)
        {
            stopCopying = true;
            pauseEvent.Set();
        }

        private void CopyFile(string sourcePath, string destinationPath, int numThreads)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(sourcePath);
                long fileLength = fileInfo.Length;
                long partSize = fileLength / numThreads;
                long remainingBytes = fileLength % numThreads;

                using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                    {
                        ManualResetEvent[] doneEvents = new ManualResetEvent[numThreads];
                        for (int i = 0; i < numThreads; i++)
                        {
                            long start = i * partSize;
                            long size = (i == numThreads - 1) ? partSize + remainingBytes : partSize;
                            doneEvents[i] = new ManualResetEvent(false);
                            ThreadPool.QueueUserWorkItem(CopyPart, new object[] { sourceStream, destStream, start, size, doneEvents[i], fileLength });
                        }
                        WaitHandle.WaitAll(doneEvents);
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Error: {ex.Message}"));
            }
        }

        private void CopyPart(object state)
        {
            object[] array = state as object[];
            FileStream sourceStream = array[0] as FileStream;
            FileStream destStream = array[1] as FileStream;
            long start = (long)array[2];
            long size = (long)array[3];
            ManualResetEvent doneEvent = array[4] as ManualResetEvent;
            long fileLength = (long)array[5];

            byte[] buffer = new byte[1024 * 1024];
            int bytesRead;

            sourceStream.Seek(start, SeekOrigin.Begin);
            destStream.Seek(start, SeekOrigin.Begin);

            long bytesCopied = 0;
            while (bytesCopied < size && !stopCopying && (bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                pauseEvent.WaitOne(); 
                destStream.Write(buffer, 0, bytesRead);
                bytesCopied += bytesRead;

                double progress = ((double)bytesCopied / fileLength) * 100;
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.Value = progress;
                    ProgressTextBlock.Text = $"Progress: {progress:F2}%";
                });
            }

            doneEvent.Set();
        }
    }
}

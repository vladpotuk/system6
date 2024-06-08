using System;
using System.Text;
using System.Threading;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private Thread numberThread;
        private Thread letterThread;
        private Thread symbolThread;
        private bool stopThreads;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartThreads_Click(object sender, RoutedEventArgs e)
        {
            stopThreads = false;

            numberThread = new Thread(GenerateNumbers) { IsBackground = true, Priority = ThreadPriority.Normal };
            letterThread = new Thread(GenerateLetters) { IsBackground = true, Priority = ThreadPriority.BelowNormal };
            symbolThread = new Thread(GenerateSymbols) { IsBackground = true, Priority = ThreadPriority.AboveNormal };

            numberThread.Start();
            letterThread.Start();
            symbolThread.Start();
        }

        private void StopThreads_Click(object sender, RoutedEventArgs e)
        {
            stopThreads = true;

            numberThread?.Join();
            letterThread?.Join();
            symbolThread?.Join();
        }

        private void GenerateNumbers()
        {
            var random = new Random();
            while (!stopThreads)
            {
                int num = random.Next(0, 100);
                Dispatcher.Invoke(() => OutputTextBox.AppendText($"Number: {num}\n"));
                Thread.Sleep(1000);
            }
        }

        private void GenerateLetters()
        {
            var random = new Random();
            while (!stopThreads)
            {
                char letter = (char)random.Next('A', 'Z' + 1);
                Dispatcher.Invoke(() => OutputTextBox.AppendText($"Letter: {letter}\n"));
                Thread.Sleep(1000);
            }
        }

        private void GenerateSymbols()
        {
            var random = new Random();
            var symbols = "!@#$%^&*()";
            while (!stopThreads)
            {
                char symbol = symbols[random.Next(symbols.Length)];
                Dispatcher.Invoke(() => OutputTextBox.AppendText($"Symbol: {symbol}\n"));
                Thread.Sleep(1000);
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            string text = TextInput.Text;
            VowelsTextBlock.Text = "Counting vowels...";
            ConsonantsTextBlock.Text = "Counting consonants...";
            SymbolsTextBlock.Text = "Counting symbols...";

            var (vowelsCount, consonantsCount, symbolsCount) = await Task.Run(() => AnalyzeText(text));

            VowelsTextBlock.Text = $"Vowels: {vowelsCount}";
            ConsonantsTextBlock.Text = $"Consonants: {consonantsCount}";
            SymbolsTextBlock.Text = $"Symbols: {symbolsCount}";
        }

        private (int, int, int) AnalyzeText(string text)
        {
            int vowelsCount = 0;
            int consonantsCount = 0;
            int symbolsCount = 0;
            string vowels = "aeiouAEIOU";

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    if (vowels.Contains(c))
                    {
                        vowelsCount++;
                    }
                    else
                    {
                        consonantsCount++;
                    }
                }
                else if (char.IsWhiteSpace(c) || char.IsPunctuation(c))
                {
                    symbolsCount++;
                }
            }

            return (vowelsCount, consonantsCount, symbolsCount);
        }
    }
}

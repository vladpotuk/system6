using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumberTextBox.Text, out int number) && number >= 0)
            {
                ProgressBar.Value = 0;
                ResultTextBlock.Text = "Calculating...";
                BigInteger result = await Task.Run(() => CalculateFactorial(number));
                ResultTextBlock.Text = $"Factorial of {number} is {result}";
                ProgressBar.Value = 100;
            }
            else
            {
                ResultTextBlock.Text = "Please enter a valid non-negative integer.";
            }
        }

        private BigInteger CalculateFactorial(int n)
        {
            BigInteger result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
    }
}

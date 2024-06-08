using System;
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
            if (double.TryParse(NumberTextBox.Text, out double number) && int.TryParse(PowerTextBox.Text, out int power))
            {
                ProgressBar.Value = 0;
                ResultTextBlock.Text = "Calculating...";
                double result = await Task.Run(() => CalculatePower(number, power));
                ResultTextBlock.Text = $"{number} to the power of {power} is {result}";
                ProgressBar.Value = 100;
            }
            else
            {
                ResultTextBlock.Text = "Please enter valid numbers.";
            }
        }

        private double CalculatePower(double number, int power)
        {
            double result = 1;
            for (int i = 0; i < power; i++)
            {
                result *= number;
            }
            return result;
        }
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaSystemResourceManager.Enums;
using AvaloniaSystemResourceManager.ViewModels;

namespace AvaloniaSystemResourceManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DiskButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is DiskInfoViewModel disk)
            {
                var vm = DataContext as MainWindowViewModel;
                if (vm != null)
                {
                    vm.UpdateDiskPerformanceMetric(disk.Name);
                }
            }
        }
    }
}
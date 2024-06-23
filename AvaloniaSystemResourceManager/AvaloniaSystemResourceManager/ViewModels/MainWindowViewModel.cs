using System.Threading.Tasks;
using System;
using System.Timers;
using ReactiveUI;

namespace AvaloniaSystemResourceManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _cpuUsage;
        private Timer _timer;

        public MainWindowViewModel()
        {
            Greeting = "xD";
            CpuUsage = "0%";
            StartCpuUsageUpdate();
        }

        public string Greeting { get; }

        public string CpuUsage
        {
            get => _cpuUsage;
            set => this.RaiseAndSetIfChanged(ref _cpuUsage, value);
        }

        private void StartCpuUsageUpdate()
        {
            _timer = new Timer(2000); // Update every 2 seconds
            _timer.Elapsed += async (sender, e) => await UpdateCpuUsage();
            _timer.Start();
        }

        private async Task UpdateCpuUsage()
        {
            var cpuUsageValue = await GetCpuUsageAsync();
            CpuUsage = $"{cpuUsageValue}%";
        }

        private async Task<int> GetCpuUsageAsync()
        {
            // Simulate an asynchronous operation to get CPU usage
            await Task.Delay(100); // Simulate delay
            var random = new Random();
            return random.Next(0, 100); // Simulate CPU usage value
        }
    }
}

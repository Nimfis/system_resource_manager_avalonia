using System.Threading.Tasks;
using System.Timers;
using ReactiveUI;
using AvaloniaSystemResourceManager.Services;

namespace AvaloniaSystemResourceManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Timer _timer;

        private string _cpuUsage;
        private string _memoryUsage;

        private CpuService _cpuService;
        private MemoryService _memoryService;

        public MainWindowViewModel()
        {
            Greeting = "xD";
            CpuUsage = "0%";
            _cpuService = new CpuService();
            _memoryService = new MemoryService();
            StartSystemResourceDiagnosticsUpdateTimer();
        }

        public string Greeting { get; }

        public string CpuUsage
        {
            get => _cpuUsage;
            set => this.RaiseAndSetIfChanged(ref _cpuUsage, value);
        }

        public string MemoryUsage
        {
            get => _memoryUsage;
            set => this.RaiseAndSetIfChanged(ref _memoryUsage, value);
        }

        private void StartSystemResourceDiagnosticsUpdateTimer()
        {
            _timer = new Timer(2000); // Update every 2 seconds
            _timer.Elapsed += async (sender, e) => await UpdateSystemResourcesUsageValues();
            _timer.Start();
        }

        private async Task UpdateSystemResourcesUsageValues()
        {
            var cpuUsageValue = await _cpuService.GetCpuUsageAsync();
            var memoryUsageValue = await _memoryService.GetMemoryUsageAsync();
            CpuUsage = $"{cpuUsageValue:F2}%";
            MemoryUsage = $"{memoryUsageValue:F2}%";
        }
    }
}

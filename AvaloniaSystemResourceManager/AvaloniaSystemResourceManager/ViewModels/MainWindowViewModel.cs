using System.Threading.Tasks;
using System.Timers;
using ReactiveUI;
using AvaloniaSystemResourceManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AvaloniaSystemResourceManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Timer _timer;

        private string _cpuUsagePercentage;
        private string _memoryAvailability;
        private string _wifiUsage;

        private CpuService _cpuService;
        private MemoryService _memoryService;
        private WifiService _wifiService;
        private GpuService _gpuService;

        public MainWindowViewModel()
        {
            CpuUsagePercentage = "-";
            _memoryAvailability = "-";
            _wifiUsage = "-";

            _cpuService = new CpuService();
            _memoryService = new MemoryService();
            _wifiService = new WifiService();
            _gpuService = new GpuService();
            StartSystemResourceDiagnosticsUpdateTimer();
        }

        public string CpuUsagePercentage
        {
            get => _cpuUsagePercentage;
            set => this.RaiseAndSetIfChanged(ref _cpuUsagePercentage, value);
        }

        public string MemoryAvailability
        {
            get => _memoryAvailability;
            set => this.RaiseAndSetIfChanged(ref _memoryAvailability, value);
        }

        public string WifiUsage
        {
            get => _wifiUsage;
            set => this.RaiseAndSetIfChanged(ref _wifiUsage, value);
        }

        public ObservableCollection<GpuInfoViewModel> GpuInfos { get; } = new ObservableCollection<GpuInfoViewModel>();

        private void StartSystemResourceDiagnosticsUpdateTimer()
        {
            _timer = new Timer(2000);
            _timer.Elapsed += async (sender, e) => await UpdateSystemResourcesUsageValues();
            _timer.Start();
        }

        private async Task UpdateSystemResourcesUsageValues()
        {
            var cpuUsagePercentageValue = await _cpuService.GetCpuUsageAsync();

            var memoryUsagePercentageValue = await _memoryService.GetMemoryUsageAsync();
            var memoryStatus = await _memoryService.GetMemoryStatusAsync();

            var wifiUsage = await _wifiService.GetWifiUsageAsync();
            var wifiSSID = await _wifiService.GetWifiSSIDAsync();

            var gpuInfoList = await _gpuService.GetGpuInfosAsync();

            CpuUsagePercentage = $"{cpuUsagePercentageValue:F2}%";
            MemoryAvailability = $"{memoryStatus.UsedMemoryGB:F2}/{memoryStatus.TotalMemoryGB:F2} GB ({memoryUsagePercentageValue:F2}%)";
            WifiUsage = $"{wifiSSID}{Environment.NewLine}Sent: {wifiUsage.SentMB:F2} MB | Received: {wifiUsage.ReceivedMB:F2} MB";

            //GpuInfos.Clear();
            foreach (var gpuInfo in gpuInfoList)
            {
                var gpuNames = GpuInfos.Select(x => x.Name);

                if (!gpuNames.Contains(gpuInfo.Name))
                {
                    GpuInfos.Add(new GpuInfoViewModel
                    {
                        Name = gpuInfo.Name,
                        MemoryGB = gpuInfo.MemoryGB
                    });
                }
            }   
        }
    }
}

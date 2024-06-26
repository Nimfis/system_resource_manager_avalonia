using System;
using System.Linq;
using System.Timers;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaSystemResourceManager.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Generic;

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
        private DiskService _diskService;

        public MainWindowViewModel()
        {
            CpuUsagePercentage = "-";
            MemoryAvailability = "-";
            WifiUsage = "-";
            _cpuService = new CpuService();
            _memoryService = new MemoryService();
            _wifiService = new WifiService();
            _gpuService = new GpuService();
            _diskService = new DiskService();

            CpuUsageSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(new List<double>() { 1, 2, 3, 4, 5, 6, 7, 10 }),
                    Fill = null
                }
            };

            StartSystemResourceDiagnosticsUpdateTimer();
        }

        public LabelVisual Title { get; set; } = new LabelVisual
        {
            Text = "Chart title",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };

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
        public ObservableCollection<DiskInfoViewModel> DiskInfos { get; } = new ObservableCollection<DiskInfoViewModel>();
        public ObservableCollection<ISeries> CpuUsageSeries { get; }

        private void StartSystemResourceDiagnosticsUpdateTimer()
        {
            _timer = new Timer(2000);
            _timer.Elapsed += async (sender, e) => await UpdateSystemResourcesUsageValues();
            _timer.Start();
        }

        private async Task UpdateSystemResourcesUsageValues()
        {
            var cpuUsagePercentageValue = await _cpuService.GetCpuUsageAsync();
            UpdateCpuUsageSeries(cpuUsagePercentageValue);
            CpuUsagePercentage = $"{cpuUsagePercentageValue:F2}%";

            var memoryUsagePercentageValue = await _memoryService.GetMemoryUsageAsync();
            var memoryStatus = await _memoryService.GetMemoryStatusAsync();
            var wifiUsage = await _wifiService.GetWifiUsageAsync();
            var wifiSSID = await _wifiService.GetWifiSSIDAsync();
            var gpuInfoList = await _gpuService.GetGpuInfosAsync();
            var diskInfoList = await _diskService.GetDiskUsageAsync();

            MemoryAvailability = $"{memoryStatus.UsedMemoryGB:F2}/{memoryStatus.TotalMemoryGB:F2} GB ({memoryUsagePercentageValue:F2}%)";
            WifiUsage = $"{wifiSSID}{Environment.NewLine}Sent: {wifiUsage.SentMB:F2} MB | Received: {wifiUsage.ReceivedMB:F2} MB";

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

            int diskIndex = 0;
            foreach (var diskInfo in diskInfoList)
            {
                var existingDiskInfo = DiskInfos.FirstOrDefault(x => x.Name == diskInfo.Name);
                if (existingDiskInfo != null)
                {
                    existingDiskInfo.WriteSpeedMBps = diskInfo.WriteSpeedMBps;
                    existingDiskInfo.ReadSpeedMBps = diskInfo.ReadSpeedMBps;
                }
                else
                {
                    DiskInfos.Add(new DiskInfoViewModel()
                    {
                        Name = diskInfo.Name,
                        DisplayName = $"Disk {diskIndex}: ({diskInfo.Name})",
                        WriteSpeedMBps = diskInfo.WriteSpeedMBps,
                        ReadSpeedMBps = diskInfo.ReadSpeedMBps,
                    });
                    diskIndex++;
                }
            }
        }

        private void UpdateCpuUsageSeries(double cpuUsage)
        {
            var series = CpuUsageSeries[0] as LineSeries<double>;
            var values = series.Values as ObservableCollection<double>;

            values.Add(cpuUsage);
            if (values.Count > 30) // Keep only the last 30 values
            {
                values.RemoveAt(0);
            }
        }
    }
}

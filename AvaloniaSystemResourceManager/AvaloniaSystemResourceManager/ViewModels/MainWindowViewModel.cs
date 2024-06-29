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
using AvaloniaSystemResourceManager.Enums;
using AvaloniaSystemResourceManager.Consts;
using System.Reactive;
using AvaloniaSystemResourceManager.Models;

namespace AvaloniaSystemResourceManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Timer _timer;
        private string _cpuUsagePercentage;
        private string _memoryAvailability;
        private string _wifiUsage;
        private ObservableCollection<ISeries> _currentResourceSeries;
        private IEnumerable<Axis> _yAxes;
        private IEnumerable<Axis> _xAxes;
        private LabelVisual _title;
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
                    Values = new ObservableCollection<double>(),
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue.WithAlpha(50)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 2 },
                },
            };

            MemoryUsageSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(),
                    Fill = new SolidColorPaint(SKColors.Purple.WithAlpha(50)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 2 },
                },
            };

            WifiUsageSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(),
                    Name = "Sent Data",
                    Fill = new SolidColorPaint(SKColors.LightBlue.WithAlpha(50)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(SKColors.LightBlue) { StrokeThickness = 2 },
                },
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(),
                    Name = "Received Data",
                    Fill = new SolidColorPaint(SKColors.LightGreen.WithAlpha(50)),
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(SKColors.LightGreen) { StrokeThickness = 2 },
                }
            };

            UpdatePerformanceMetricCommand = ReactiveCommand.Create<PerformanceMetric>(SetSelectedMetric);
            SetSelectedMetric(PerformanceMetric.CPU);

            StartSystemResourceDiagnosticsUpdateTimer();
        }

        public ReactiveCommand<PerformanceMetric, Unit> UpdatePerformanceMetricCommand { get; }

        public IEnumerable<Axis> YAxes
        {
            get => _yAxes;
            set => this.RaiseAndSetIfChanged(ref _yAxes, value);
        }

        public IEnumerable<Axis> XAxes
        {
            get => _xAxes;
            set => this.RaiseAndSetIfChanged(ref _xAxes, value);
        }

        public LabelVisual Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
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
        public ObservableCollection<DiskInfoViewModel> DiskInfos { get; } = new ObservableCollection<DiskInfoViewModel>();

        public ObservableCollection<ISeries> CurrentResourceSeries
        {
            get => _currentResourceSeries;
            set => this.RaiseAndSetIfChanged(ref _currentResourceSeries, value);
        }

        public ObservableCollection<ISeries> CpuUsageSeries { get; }
        public ObservableCollection<ISeries> MemoryUsageSeries { get; }
        public ObservableCollection<ISeries> WifiUsageSeries { get; }
        public Dictionary<string, ObservableCollection<ISeries>> DiskUsageSeries { get; } = new Dictionary<string, ObservableCollection<ISeries>>();

        private void StartSystemResourceDiagnosticsUpdateTimer()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += async (sender, e) => await UpdateSystemResourcesUsageValues();
            _timer.Start();
        }

        private async Task UpdateSystemResourcesUsageValues()
        {
            try
            {
                var cpuUsagePercentageValue = await _cpuService.GetCpuUsageAsync();
                CpuUsagePercentage = $"{cpuUsagePercentageValue:F2}%";

                var memoryUsagePercentageValue = await _memoryService.GetMemoryUsageAsync();
                var memoryStatus = await _memoryService.GetMemoryStatusAsync();
                var wifiUsage = await _wifiService.GetCurrentWifiUsagePerSecondAsync();
                var wifiSSID = await _wifiService.GetWifiSSIDAsync();
                var gpuInfoList = await _gpuService.GetGpuInfosAsync();
                var diskInfoList = await _diskService.GetDiskUsageAsync();

                MemoryAvailability = $"{memoryStatus.UsedMemoryGB:F2}/{memoryStatus.TotalMemoryGB:F2} GB ({memoryUsagePercentageValue:F2}%)";
                WifiUsage = $"{wifiSSID}{Environment.NewLine}Sent: {wifiUsage.SentMBPerSecond:F2} MB | Received: {wifiUsage.ReceivedMBPerSecond:F2} MB";

                GetGpuInfos(gpuInfoList);
                GetDiskInfos(diskInfoList);

                UpdateCpuUsageSeries(cpuUsagePercentageValue);
                UpdateMemoryUsageSeries(memoryUsagePercentageValue);
                UpdateWifiUsageSeries(wifiUsage.SentMBPerSecond, wifiUsage.ReceivedMBPerSecond);
            }
            catch (Exception)
            {

            }
        }

        private void GetGpuInfos(IEnumerable<GpuInfo> gpuInfoList)
        {
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

        private void GetDiskInfos(IEnumerable<DiskInfo> diskInfoList) 
        {
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
            if (values.Count >= PerformanceChartConfiguration.MAX_CHART_SECONDS)
            {
                values.RemoveAt(0);
            }
        }

        private void UpdateMemoryUsageSeries(double memoryUsage)
        {
            var series = MemoryUsageSeries[0] as LineSeries<double>;
            var values = series.Values as ObservableCollection<double>;

            values.Add(memoryUsage);
            if (values.Count >= PerformanceChartConfiguration.MAX_CHART_SECONDS)
            {
                values.RemoveAt(0);
            }
        }

        private void UpdateWifiUsageSeries(double sentUsage, double receivedUsage)
        {
            var sentSeries = WifiUsageSeries[0] as LineSeries<double>;
            var sentValues = sentSeries.Values as ObservableCollection<double>;

            sentValues.Add(sentUsage);
            if (sentValues.Count >= PerformanceChartConfiguration.MAX_CHART_SECONDS)
            {
                sentValues.RemoveAt(0);
            }

            var receivedSeries = WifiUsageSeries[1] as LineSeries<double>;
            var receivedValues = receivedSeries.Values as ObservableCollection<double>;

            receivedValues.Add(receivedUsage);
            if (receivedValues.Count >= PerformanceChartConfiguration.MAX_CHART_SECONDS)
            {
                receivedValues.RemoveAt(0);
            }
        }

        private void SetChartConfiguration(PerformanceMetric selectedMetric)
        {
            var chartConfig = PerformanceChartConfiguration.GetPerformanceChartConfiguration(selectedMetric);

            XAxes = chartConfig.XAxes;
            YAxes = chartConfig.YAxes;
            Title = chartConfig.Title;

            SetCurrentResourceSeries(selectedMetric);
        }

        private void SetSelectedMetric(PerformanceMetric metric)
        {
            SetChartConfiguration(metric);
        }

        private void SetCurrentResourceSeries(PerformanceMetric selectedMetric)
        {
            switch (selectedMetric)
            {
                case PerformanceMetric.CPU:
                    CurrentResourceSeries = CpuUsageSeries;
                    break;
                case PerformanceMetric.RAM:
                    CurrentResourceSeries = MemoryUsageSeries;
                    break;
                case PerformanceMetric.WiFi:
                    CurrentResourceSeries = WifiUsageSeries;
                    break;
                default:
                    break;
            }
        }
    }
}

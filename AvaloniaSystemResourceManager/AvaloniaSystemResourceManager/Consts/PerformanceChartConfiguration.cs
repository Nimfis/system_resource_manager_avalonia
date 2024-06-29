using SkiaSharp;
using System.Collections.Generic;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using AvaloniaSystemResourceManager.Enums;
using System;

namespace AvaloniaSystemResourceManager.Consts
{
    internal class PerformanceChartConfiguration
    {
        public const int MAX_CHART_SECONDS = 60;
        public const int MAX_CHART_PERCENTAGE = 100;

        public static (IEnumerable<Axis> XAxes, IEnumerable<Axis> YAxes, LabelVisual Title) GetPerformanceChartConfiguration(PerformanceMetric metric)
        {
            var xAxes = new List<Axis>();
            var yAxes = new List<Axis>();
            var title = new LabelVisual();

            switch (metric)
            {
                case PerformanceMetric.CPU:
                    xAxes.Add(CPUXAxis());
                    yAxes.Add(CPUYAxis());
                    title = CreateTitle("CPU Performance:");
                    break;
                case PerformanceMetric.RAM:
                    xAxes.Add(MemoryXAxis());
                    yAxes.Add(MemoryYAxis());
                    title = CreateTitle("Ram Usage:");
                    break;
                case PerformanceMetric.WiFi:
                    xAxes.Add(CreateAxis(MAX_CHART_SECONDS, "s"));
                    yAxes.Add(CreateAxis(10, "MB/s", isDataRateAxis: true));
                    title = CreateTitle("WiFi Usage:");
                    break;
                default:
                    break;
            }

            return (xAxes, yAxes, title);
        }

        private static Axis CPUXAxis() => CreateAxis(MAX_CHART_SECONDS, "s");
        private static Axis CPUYAxis() => CreateAxis(MAX_CHART_PERCENTAGE, "%");
        private static Axis MemoryXAxis() => CreateAxis(MAX_CHART_SECONDS, "s");
        private static Axis MemoryYAxis() => CreateAxis(MAX_CHART_PERCENTAGE, "%");

        private static Axis CreateAxis(int maxLimit, string suffix, bool isDataRateAxis = false, bool isVisible = true)
        {
            return new Axis
            {
                MinLimit = 0,
                MaxLimit = maxLimit,
                Labeler = isDataRateAxis ? FormatDataRateLabel : (value => value.ToString("0") + suffix),
                Position = LiveChartsCore.Measure.AxisPosition.Start,
                IsVisible = isVisible,
                LabelsRotation = 0
            };
        }

        private static LabelVisual CreateTitle(string text)
        {
            return new LabelVisual
            {
                Text = text,
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };
        }

        private static string FormatDataRateLabel(double value)
        {
            // Convert to KB/s if the MB value is less than 0.01 MB/s (10 KB/s)
            if (value < 0.01)
                return (value * 1024).ToString("0.00") + " KB/s";
            else
                return value.ToString("0.00") + " MB/s";
        }
    }
}

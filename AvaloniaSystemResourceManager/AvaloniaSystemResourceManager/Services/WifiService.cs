using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Timers;

namespace AvaloniaSystemResourceManager.Services
{
    internal class WifiService
    {
        private NetworkInterface _wifiInterface;
        private IPv4InterfaceStatistics _previousStats;
        private Timer _updateTimer;
        private double _lastSentMBPerSecond;
        private double _lastReceivedMBPerSecond;

        public WifiService()
        {
            _wifiInterface = GetWifiInterface();
            if (_wifiInterface == null)
            {
                throw new InvalidOperationException("No Wi-Fi network interface found.");
            }

            UpdateCurrentStats(); // Initial update
            _updateTimer = new Timer(1000); // Set the timer to update every second
            _updateTimer.Elapsed += UpdateUsagePerSecond;
            _updateTimer.Start();
        }

        private void UpdateUsagePerSecond(object sender, ElapsedEventArgs e)
        {
            var currentStats = _wifiInterface.GetIPv4Statistics();
            if (_previousStats != null)
            {
                var sentBytes = currentStats.BytesSent - _previousStats.BytesSent;
                var receivedBytes = currentStats.BytesReceived - _previousStats.BytesReceived;

                _lastSentMBPerSecond = sentBytes / (1024.0 * 1024.0);
                _lastReceivedMBPerSecond = receivedBytes / (1024.0 * 1024.0);
            }
            _previousStats = currentStats; // Update the previous stats for the next calculation
        }

        public Task<(double SentMBPerSecond, double ReceivedMBPerSecond)> GetCurrentWifiUsagePerSecondAsync()
        {
            return Task.FromResult((_lastSentMBPerSecond, _lastReceivedMBPerSecond));
        }

        private NetworkInterface GetWifiInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .FirstOrDefault(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                                                         ni.OperationalStatus == OperationalStatus.Up);
        }

        private void UpdateCurrentStats()
        {
            _previousStats = _wifiInterface.GetIPv4Statistics();
        }

        public async Task<string> GetWifiSSIDAsync()
        {
            return await Task.Run(() =>
            {
                var ssid = _wifiInterface.Description;
                return ssid;
            });
        }
    }
}

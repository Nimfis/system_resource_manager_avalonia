using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace AvaloniaSystemResourceManager.Services
{
    internal class WifiService
    {
        private NetworkInterface _wifiInterface;

        public WifiService()
        {
            _wifiInterface = GetWifiInterface();
            if (_wifiInterface == null)
            {
                throw new InvalidOperationException("No Wi-Fi network interface found.");
            }
        }

        private NetworkInterface GetWifiInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .FirstOrDefault(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                                                         ni.OperationalStatus == OperationalStatus.Up);
        }

        public async Task<(double SentMB, double ReceivedMB)> GetWifiUsageAsync()
        {
            return await Task.Run(() =>
            {
                var stats = _wifiInterface.GetIPv4Statistics();
                double sentMB = stats.BytesSent / (1024.0 * 1024.0);
                double receivedMB = stats.BytesReceived / (1024.0 * 1024.0);
                return (sentMB, receivedMB);
            });
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

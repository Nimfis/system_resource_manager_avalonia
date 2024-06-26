using System;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Collections.Generic;
using AvaloniaSystemResourceManager.Models;

namespace AvaloniaSystemResourceManager.Services
{
    internal class DiskService
    {
        public async Task<List<DiskInfo>> GetDiskUsageAsync()
        {
            return await Task.Run(() =>
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk");
                var diskData = searcher.Get()
                                       .Cast<ManagementObject>()
                                       .Where(mo => mo["Name"].ToString() != "_Total")
                                       .Select(mo => (
                                           WriteSpeedMBps: Convert.ToDouble(mo["DiskWriteBytesPerSec"]) / (1024.0 * 1024.0),
                                           ReadSpeedMBps: Convert.ToDouble(mo["DiskReadBytesPerSec"]) / (1024.0 * 1024.0),
                                           Name: mo["Name"].ToString()
                                       ))
                                       .ToArray();

                return diskData.Select(x => {
                    return new DiskInfo()
                    {
                        Name = x.Name,
                        ReadSpeedMBps = x.ReadSpeedMBps,
                        WriteSpeedMBps = x.WriteSpeedMBps
                    };
                }).ToList();
            });
        }

        public async Task<string[]> GetDiskNamesAsync()
        {
            return await Task.Run(() =>
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                var diskNames = searcher.Get()
                                        .Cast<ManagementObject>()
                                        .Select(mo => mo["Model"].ToString())
                                        .ToArray();
                return diskNames;
            });
        }
    }
}

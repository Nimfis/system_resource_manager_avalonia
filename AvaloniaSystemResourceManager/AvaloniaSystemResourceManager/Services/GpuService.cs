using System;
using System.Management;
using System.Threading.Tasks;
using System.Collections.Generic;
using AvaloniaSystemResourceManager.Models;
using System.Linq;

namespace AvaloniaSystemResourceManager.Services
{
    internal class GpuService
    {
        public async Task<List<GpuInfo>> GetGpuInfosAsync()
        {
            return await Task.Run(() =>
            {
                var gpuInfos = new List<GpuInfo>();

                try
                {
                    using (var searcher = new ManagementObjectSearcher("select Name, AdapterRAM, PNPDeviceID, DriverVersion from Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var name = obj["Name"];
                            var adapterRAM = obj["AdapterRAM"] != null ? Convert.ToDouble(obj["AdapterRAM"]) / (1024.0 * 1024 * 1024) : 0.0; // Convert to GB
                            var pnpDeviceID = obj["PNPDeviceID"] as string;
                            var driverVersion = obj["DriverVersion"] as string;

                            if (name == null || pnpDeviceID == null || driverVersion == null) continue;

                            gpuInfos.Add(new GpuInfo
                            {
                                Name = name as string,
                                MemoryGB = adapterRAM
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving GPU information: {ex.Message}");
                }

                return gpuInfos;
            });
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AvaloniaSystemResourceManager.Services
{
    public class MemoryService
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                dwMemoryLoad = 0;
                ullTotalPhys = 0;
                ullAvailPhys = 0;
                ullTotalPageFile = 0;
                ullAvailPageFile = 0;
                ullTotalVirtual = 0;
                ullAvailVirtual = 0;
                ullAvailExtendedVirtual = 0;
            }
        }

        public async Task<double> GetMemoryUsageAsync()
        {
            return await Task.Run(() =>
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    return (double)(memStatus.ullTotalPhys - memStatus.ullAvailPhys) / memStatus.ullTotalPhys * 100;
                }
                else
                {
                    throw new InvalidOperationException("Unable to get memory status.");
                }
            });
        }

        public async Task<(double TotalMemoryGB, double UsedMemoryGB, double AvailableMemoryGB)> GetMemoryStatusAsync()
        {
            return await Task.Run(() =>
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    double totalMemoryGB = memStatus.ullTotalPhys / (1024.0 * 1024 * 1024);
                    double usedMemoryGB = (memStatus.ullTotalPhys - memStatus.ullAvailPhys) / (1024.0 * 1024 * 1024);
                    double availableMemoryGB = memStatus.ullAvailPhys / (1024.0 * 1024 * 1024);
                    return (totalMemoryGB, usedMemoryGB, availableMemoryGB);
                }
                else
                {
                    throw new InvalidOperationException("Unable to get memory status.");
                }
            });
        }
    }
}

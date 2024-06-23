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
            public MEMORYSTATUSEX(bool? filler) : this() { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
        }

        public async Task<double> GetMemoryUsageAsync()
        {
            return await Task.Run(() =>
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
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
    }
}

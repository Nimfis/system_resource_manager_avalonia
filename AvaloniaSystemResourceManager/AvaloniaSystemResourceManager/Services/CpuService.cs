using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AvaloniaSystemResourceManager.Services
{
    internal class CpuService
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        private FILETIME _prevIdleTime;
        private FILETIME _prevKernelTime;
        private FILETIME _prevUserTime;

        public CpuService()
        {
            GetSystemTimes(out _prevIdleTime, out _prevKernelTime, out _prevUserTime);
        }

        public async Task<double> GetCpuUsageAsync()
        {
            return await Task.Run(() =>
            {
                GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime);

                ulong prevIdle = ((ulong)_prevIdleTime.dwHighDateTime << 32) | _prevIdleTime.dwLowDateTime;
                ulong prevKernel = ((ulong)_prevKernelTime.dwHighDateTime << 32) | _prevKernelTime.dwLowDateTime;
                ulong prevUser = ((ulong)_prevUserTime.dwHighDateTime << 32) | _prevUserTime.dwLowDateTime;

                ulong idle = ((ulong)idleTime.dwHighDateTime << 32) | idleTime.dwLowDateTime;
                ulong kernel = ((ulong)kernelTime.dwHighDateTime << 32) | kernelTime.dwLowDateTime;
                ulong user = ((ulong)userTime.dwHighDateTime << 32) | userTime.dwLowDateTime;

                ulong totalSystem = (kernel - prevKernel) + (user - prevUser);
                ulong totalIdle = idle - prevIdle;

                _prevIdleTime = idleTime;
                _prevKernelTime = kernelTime;
                _prevUserTime = userTime;

                if (totalSystem == 0)
                {
                    return 0.0;
                }

                return (1.0 - ((double)totalIdle / totalSystem)) * 100;
            });
        }
    }
}

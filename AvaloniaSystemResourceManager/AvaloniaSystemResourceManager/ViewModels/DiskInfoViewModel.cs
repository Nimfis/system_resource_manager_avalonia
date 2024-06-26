using ReactiveUI;

namespace AvaloniaSystemResourceManager.ViewModels
{
    public class DiskInfoViewModel : ViewModelBase
    {

        private string _name;
        private string _displayName;
        private double _writeSpeedMBps;
        private double _readSpeedMBps;

        public string Name 
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        public double WriteSpeedMBps
        {
            get => _writeSpeedMBps;
            set => this.RaiseAndSetIfChanged(ref _writeSpeedMBps, value);
        }

        public double ReadSpeedMBps
        {
            get => _readSpeedMBps;
            set => this.RaiseAndSetIfChanged(ref _readSpeedMBps, value);
        }
    }
}

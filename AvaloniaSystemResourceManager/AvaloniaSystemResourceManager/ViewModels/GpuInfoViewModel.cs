using ReactiveUI;

namespace AvaloniaSystemResourceManager.ViewModels
{
    public class GpuInfoViewModel : ViewModelBase
    {
        private string _name;
        private double _memoryGB;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public double MemoryGB
        {
            get => _memoryGB;
            set => this.RaiseAndSetIfChanged(ref _memoryGB, value);
        }
    }
}

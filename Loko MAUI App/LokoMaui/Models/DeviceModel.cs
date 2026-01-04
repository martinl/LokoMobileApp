using CommunityToolkit.Mvvm.ComponentModel;

namespace loko.Models
{
    public partial class DeviceModel : ObservableObject
    {
        [ObservableProperty]
        private string _label;

        [ObservableProperty]
        private bool _isSelected;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace loko.Models;

public partial class SettingsOptions : ObservableObject
{
    [ObservableProperty]
    private string _label;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isOneSelection = true;

    [ObservableProperty]
    private bool _isClickedCommand;

    [ObservableProperty]
    private ICommand _command;

    [ObservableProperty]
    private ObservableCollection<SettingsOptionsContent> _contents;

    [ObservableProperty]
    private bool _isNeedToShowContent = true;

    [ObservableProperty]
    private bool _isScrolable;
}

public partial class SettingsOptionsContent : ObservableObject
{
    [ObservableProperty]
    private string _label;

    [ObservableProperty]
    private ICommand _command;

    [ObservableProperty]
    private bool _isNeedToShowToggle = true;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private SettingsType _settingsType;
}

public enum SettingsType { Selection, MapType, BLE, Dates, OfflineMap }

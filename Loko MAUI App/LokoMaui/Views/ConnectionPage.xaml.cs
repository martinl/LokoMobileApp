using loko.ViewModels;
using CommunityToolkit.Maui.Views;
using loko.Framework;

namespace loko.Views;

public partial class ConnectionPage : BaseContentPage<ConnectionPageViewModel>
{

    public ConnectionPage(ConnectionPageViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ConnectionPageViewModel viewModel)
        {
            viewModel.StartScanningForDevices();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();



        if (BindingContext is ConnectionPageViewModel viewModel)
        {
            viewModel.StopScanningForDevices();
        }
    }

    private void Expander_Tapped(object sender, System.EventArgs e)
    {
        Expander expander = sender as Expander;
        expander.ForceLayout();
    }
}
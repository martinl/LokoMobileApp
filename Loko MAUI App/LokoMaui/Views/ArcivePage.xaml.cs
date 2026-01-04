using loko.Framework;
using loko.ViewModels;

namespace loko.Views;

public partial class ArcivePage : BaseContentPage<ArchivePageViewModel>
{    
    
    public ArcivePage(ArchivePageViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }    

}
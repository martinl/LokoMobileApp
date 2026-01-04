using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loko.Framework
{
    public abstract class BaseContentPage<T> : ContentPage where T : BaseViewModel
    {
        protected BaseContentPage(T viewModel)
        {
            BindingContext = ViewModel = viewModel;
            Title = viewModel.Title;
//            On<iOS>().SetUseSafeArea(true);
//#if ANDROID || IOS
//            Behaviors.Add(new CommunityToolkit.Maui.Behaviors.StatusBarBehavior
//            {
//                StatusBarColor = BackgroundColor
//            });
//#endif
        }

        protected T ViewModel { get; }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.InitializeAsync();
        }

        protected override void OnDisappearing()
        {
            ViewModel.UnInitializeAsync();
            base.OnDisappearing();
        }
    }
}

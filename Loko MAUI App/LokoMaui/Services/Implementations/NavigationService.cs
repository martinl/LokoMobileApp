using loko.Framework;
using loko.Services.Interfaces;
using loko.ViewModels;
using loko.Views;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace loko.Services.Implementations
{
    public class NavigationService : INavigationService
    {
        readonly IServiceProvider _services;

        protected INavigation Navigation
        {
            get
            {
                INavigation? navigation = Application.Current?.MainPage?.Navigation;
                if (navigation is not null)
                    return navigation;
                else
                {
                    //This is not good!
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    throw new Exception();
                }
            }
        }

        public NavigationService(IServiceProvider services)
            => _services = services;        

        public Task NavigateToMapPage()
                => NavigateToPage<MapPage>();

        public Task NavigateToArcivePage(object? parameter)
                => NavigateToPage<ArcivePage>(parameter);

        public Task NavigateToDownloadMapPage()
                => NavigateToPage<DownloadMapPage>();

        public Task NavigateToMyOfflineMapsPage()
                => NavigateToPage<MyOfflineMapsPage>();

        public async Task ChangeRootPage(Type pageType)
        {
            var newRootPage = _services.GetService(pageType) as Page;
            if (newRootPage != null)
            {
                Application.Current.MainPage = new NavigationPage(newRootPage);
            }
            else
            {
                throw new InvalidOperationException($"Unable to resolve type {pageType.FullName}");
            }
        }
        public async Task NavigateToConnectionPage()
        {
            try
            {
                await NavigateToPage<ConnectionPage>();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error navigating to ConnectionPage: {ex.Message}");
                // Handle the error appropriately
            }
        }

        public Task ChangeRootPage<T>() where T : Page
        {
            var newRootPage = ResolvePage<T>();
            if (newRootPage is not null)
            {
                Application.Current.MainPage = new NavigationPage(newRootPage);
                return Task.CompletedTask;
            }
            else
                throw new InvalidOperationException($"Unable to resolve type {typeof(T).FullName}");
        }


        public Task NavigateBack()
        {
            if (Navigation.NavigationStack.Count > 1)
                return Navigation.PopAsync();

            throw new InvalidOperationException("No pages to navigate back to!");
        }

        private async Task NavigateToPage<T>(object? parameter = null) where T : Page
        {
            var toPage = ResolvePage<T>();

            if (toPage is not null)
            {
                //Subscribe to the toPage's NavigatedTo event
                

                //Get VM of the toPage
                var toViewModel = GetPageViewModelBase(toPage);

                //Call navigatingTo on VM, passing in the paramter
                if (parameter is not null)
                {
                    var ob = (parameter as Dictionary<string, object>)["Title"].ToString();
                    toViewModel.Title = ob;
                    (toPage.BindingContext as ArchivePageViewModel).Title = ob;
                }

                //Navigate to requested page
                await Navigation.PushAsync(toPage, true);
            }
            else
                throw new InvalidOperationException($"Unable to resolve type {typeof(T).FullName}");
        }       
        

        private BaseViewModel? GetPageViewModelBase(Page? p)
            => p?.BindingContext as BaseViewModel;

        private T? ResolvePage<T>() where T : Page
            => _services.GetService<T>();


    }
}

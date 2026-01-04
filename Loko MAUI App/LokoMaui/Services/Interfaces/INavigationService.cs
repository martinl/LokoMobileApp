using loko.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loko.Services.Interfaces
{
    public interface INavigationService
    {
        Task NavigateBack();
        Task NavigateToMapPage();
        Task NavigateToArcivePage(object? parameter);
        Task NavigateToConnectionPage();
        Task ChangeRootPage<T>() where T : Page;

        Task ChangeRootPage(Type pageType);

        Task NavigateToDownloadMapPage();
        Task NavigateToMyOfflineMapsPage();
    }
}

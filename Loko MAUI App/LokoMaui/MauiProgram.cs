using CommunityToolkit.Maui;
using loko.Services.Implementations;
using loko.Services.Interfaces;
using loko.ViewModels;
using loko.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Handlers;

namespace loko;
public static class MauiProgram
{
    public static IBLEConnectionService BleConnectionService { get; set; }
    public static Microsoft.Maui.Maps.MapType MapType { get; set; } = Microsoft.Maui.Maps.MapType.Street;
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseSkiaSharp()
            .UseMauiCompatibility()
            .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("Montserrat-Thin.ttf", "MontserratThin");
            fonts.AddFont("Montserrat-Light.ttf", "MontserratLightFont");
            fonts.AddFont("Montserrat-Medium.ttf", "MontserratMediumFont");
            fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegularFont");
        })
            .UseMauiCommunityToolkit()
            .RegisterAppServices()
            .RegisterViewModels()
            .RegisterViews();
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>();
            handlers.AddHandler(typeof(SKCanvasView), typeof(SKCanvasViewHandler));
        });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<IBLEConnectionService, BLEConnectionService>();
        mauiAppBuilder.Services.AddSingleton<INavigationService, NavigationService>();
        mauiAppBuilder.Services.AddSingleton<IPermissionService, PermissionService>();
        //mauiAppBuilder.Services.AddSingleton<LocationService>();
        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<MapViewModel>();
        mauiAppBuilder.Services.AddSingleton<ArchivePageViewModel>();
        mauiAppBuilder.Services.AddSingleton<ConnectionPageViewModel>();

        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
    {
        //mauiAppBuilder.Services.AddSingleton<MapPage>();
        mauiAppBuilder.Services.AddTransient<MapPage>();
        mauiAppBuilder.Services.AddTransient<OSMPage>();
        mauiAppBuilder.Services.AddSingleton<ArcivePage>();
        mauiAppBuilder.Services.AddTransient<ConnectionPage>();
        mauiAppBuilder.Services.AddSingleton<DownloadMapPage>();
        mauiAppBuilder.Services.AddTransient<MyOfflineMapsPage>();

        return mauiAppBuilder;
    }
}
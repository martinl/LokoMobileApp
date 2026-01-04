using loko.Helpers;
using loko.Views;
using Microsoft.Extensions.DependencyInjection;

namespace loko
{
    public partial class App : Application
    {
        private static bool _useOSM = false;
        public static bool UseOSM
        {
            get => _useOSM;
            set
            {
                if (_useOSM != value)
                {
                    _useOSM = value;
                    OnUseOSMChanged?.Invoke(null, value);
                    // Save the preference when it changes
                    AppPreferences.SetSelectedMapType(value ? "OSM" : "Google");
                }
            }
        }

        public static event EventHandler<bool> OnUseOSMChanged;

        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // Load the saved map type preference
            _useOSM = AppPreferences.GetSelectedMapType() == "OSM";

            // Set the main page based on the saved preference
            SetInitialPage();

            // Subscribe to the OnUseOSMChanged event to update the main page when the map type changes
            OnUseOSMChanged += OnUseOSMChangedHandler;
        }

        private void SetInitialPage()
        {
            Page mainPage = UseOSM
                ? _serviceProvider.GetRequiredService<OSMPage>()
                : _serviceProvider.GetRequiredService<MapPage>();

            MainPage = new NavigationPage(mainPage);
        }

        private void OnUseOSMChangedHandler(object sender, bool useOSM)
        {
            // Update the main page when the map type changes
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetInitialPage();
            });
        }

        protected override void OnStart()
        {
            base.OnStart();
            // Any additional startup logic
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            // Any logic for when the app goes to sleep
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Any logic for when the app resumes
        }
    }
}
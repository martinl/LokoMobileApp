using Microsoft.Maui.Storage;

namespace loko.Helpers
{
    public static class AppPreferences
    {
        private const string MapTypeKey = "SelectedMapType";

        public static void SetSelectedMapType(string mapType)
        {
            Preferences.Set(MapTypeKey, mapType);
        }

        public static string GetSelectedMapType()
        {
            return Preferences.Get(MapTypeKey, "Google"); // Default to Google if not set
        }
    }
}

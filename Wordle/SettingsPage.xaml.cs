using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Wordle
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            themeSwitch.IsToggled = Preferences.Get("isDarkMode", false);
        }

        private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
        {
            bool isDarkMode = e.Value;
            Preferences.Set("isDarkMode", isDarkMode);
            App.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
        }
    }
}


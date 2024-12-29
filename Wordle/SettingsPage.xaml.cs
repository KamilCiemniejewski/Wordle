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
            string savedDifficulty = Preferences.Get("Difficulty", "Moderate");
            difficultySelect.SelectedItem = savedDifficulty;
        }

        private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
        {
            bool isDarkMode = e.Value;
            Preferences.Set("isDarkMode", isDarkMode);
            App.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        private void OnDifficultyChanged(object sender, EventArgs e)
        {
            if (difficultySelect.SelectedIndex == -1) return;

            string selectedDifficulty = difficultySelect.SelectedItem.ToString();

            Preferences.Set("Difficulty", selectedDifficulty);

            DisplayAlert("You changed difficulty", $"Game difficulty has been set to {selectedDifficulty}.", "OK");
        }
    }
}


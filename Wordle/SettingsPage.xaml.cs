using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Wordle
{
    public partial class SettingsPage : ContentPage
    {
        //Saving settings
        public SettingsPage()
        {
            InitializeComponent();
            themeSwitch.IsToggled = Preferences.Get("isDarkMode", false);
            string savedDifficulty = Preferences.Get("Difficulty", "Moderate");
            difficultySelect.SelectedItem = savedDifficulty;
        }

        //Switch between light and dark mode
        private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
        {
            bool isDarkMode = e.Value;
            Preferences.Set("isDarkMode", isDarkMode);
            App.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        //Allow user to change difficulty
        private void OnDifficultyChanged(object sender, EventArgs e)
        {
            if (difficultySelect.SelectedIndex == -1) return;

            string selectedDifficulty = difficultySelect.SelectedItem.ToString();

            string currentDifficulty = Preferences.Get("Difficulty", "Moderate");
            if (selectedDifficulty != currentDifficulty)
            {
                Preferences.Set("Difficulty", selectedDifficulty);
                DisplayAlert("You changed difficulty", $"Game difficulty has been set to {selectedDifficulty}.", "OK");
            }
        }
    }
}


using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Wordle
{
    public partial class MainPage : ContentPage
    {
        private const string nameOfFile = "words.txt";
        private const string urlFile = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
        private String filePathLocal => Path.Combine(FileSystem.AppDataDirectory, nameOfFile);

        private string chosenWord;
        private int attemptsMade = 0;

        public MainPage()
        {
            InitializeComponent();
            LoadingWordList();
        }

        private async void InitializingGame()
        {
            try
            {
                await LoadingWordList();
                SelectingRandomWord();
                attemptsMade = 0;
                FeedbackLabel.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                FeedbackLabel.Text = "Game could not have been initialized.";
            }
        }
        private async Task LoadingWordList()
        {
            if (!File.Exists(filePathLocal))
            {
                await DownloadingWordListAsync();
            }
        }


        private async Task DownloadingWordListAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                var WordsContent = await httpClient.GetStringAsync(urlFile);

                File.WriteAllText(filePathLocal, WordsContent);
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"Failed to download the list of words: {ex.Message}");
                throw;
            }
        }

        private string ReadingWordList()
        {
            return File.Exists(filePathLocal) ? File.ReadAllText(filePathLocal) : string.Empty;
        }

        private void SelectingRandomWord()
        {
            string[] words = ReadingWordList().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Random random = new Random();
            chosenWord = words[random.Next(words.Length)];
            Console.WriteLine($"DEBUG: The word that has been selected is {chosenWord}");
        }

        private void OnGuessSubmit(object sender, EventArgs e)
        {
            string guess = $"{Letter1.Text}{Letter2.Text}{Letter3.Text}{Letter4.Text}{Letter5.Text}";

            if (guess.Length != 5 || !guess.All(char.IsLetter))
            {
                FeedbackLabel.Text = "Please enter a 5 letter word.";
                return;
            }
        }
    }
}

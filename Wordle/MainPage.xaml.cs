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
        private String filePathLocal => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameOfFile);

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

            string wordListContent = ReadingWordList();
            if (string.IsNullOrEmpty(wordListContent))
            {
                Console.WriteLine("The word list is empty or failed to load.");
                FeedbackLabel.Text = "Word list failed to load.";
            }
            else
            {
                Console.WriteLine("Word list loaded successfully");
            }
        }


        private async Task DownloadingWordListAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                var WordsContent = await httpClient.GetStringAsync(urlFile);

                if (!string.IsNullOrWhiteSpace(WordsContent))
                {
                    File.WriteAllText(filePathLocal, WordsContent);
                    Console.WriteLine($"File has been downloaded and saved to: {filePathLocal}");
                }
                else
                {
                    throw new Exception("The content that was downloaded is empty.");
                }

            }
            catch ( Exception ex )
            {
                Console.WriteLine($"Failed to download the list of words: {ex.Message}");
                throw;
            }
        }

        private string ReadingWordList()
        {
            if (!File.Exists(filePathLocal))
            {
                Console.WriteLine($"File not found at: {filePathLocal}");
                return string.Empty;
            }

            try
            {
                string content = File.ReadAllText(filePathLocal);
                Console.WriteLine("File content successfully read.");
                return content;
            }

            catch (FileNotFoundException)
            {
                Console.WriteLine($"File not found: {filePathLocal}");
            }

            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access to the file is unauthorized: {filePathLocal}");
            }

            catch (IOException ex)
            {
                Console.WriteLine($"An I/O error occured: {ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occured: {ex.Message}");
            }

            return string.Empty;
        }

        private void SelectingRandomWord()
        {
            string wordListContent = ReadingWordList();

            if (string.IsNullOrEmpty(wordListContent))
            {
                FeedbackLabel.Text = "The list of words is empty or failed to load.";
                return;
            }

            string[] words = wordListContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                FeedbackLabel.Text = "Nothing available in the word list.";
                return;
            }

            Random random = new Random();
            chosenWord = words[random.Next(words.Length)].ToUpper();
            Console.WriteLine($"Selected word is {chosenWord}");
        }

        private void OnGuessSubmit(object sender, EventArgs e)
        {
            string guess = $"{Letter1.Text}{Letter2.Text}{Letter3.Text}{Letter4.Text}{Letter5.Text}".ToUpper();

            if (guess.Length != 5 || !guess.All(char.IsLetter))
            {
                FeedbackLabel.Text = "Please enter a 5 letter word.";
                return;
            }

            ProcessGuess(guess);
        }

        private void ProcessGuess(string guess)
        {
            attemptsMade++;

            FeedbackLabel.Text = $"You guessed: {guess}";

            if (guess == chosenWord)
            {
                FeedbackLabel.Text = $"You managed to guess the word in {attemptsMade} attempts! Well done!";
            }

            else if (attemptsMade >= 6)
            {
                FeedbackLabel.Text = $"Bad luck! The word was {chosenWord}.";
            }

            else
            {
                FeedbackLabel.Text = $"Try again! Attempts left: {attemptsMade}";
            }
        }
    }
}

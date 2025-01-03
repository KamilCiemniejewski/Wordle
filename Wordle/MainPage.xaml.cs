﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Plugin.Maui.Audio;
using Microsoft.Maui.Controls;

//https://github.com/KamilCiemniejewski/Wordle
namespace Wordle
{
    public partial class MainPage : ContentPage
    {
        //Variables
        private const string nameOfFile = "words.txt";
        private const string urlFile = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
        private const string historyFile = "history.json";
        private const string playerFile = "player.json";
        private String filePathLocal => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameOfFile);
        private String filePathHistory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), historyFile);
        private String filePathPlayer => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), playerFile);

        private string chosenWord;
        private int attemptsMade = 0;
        private int attemptsMax = 6;
        private string playerName;
        private List<GameResult> gameHistory = new();

        private readonly IAudioManager audioManager;

        public MainPage()
        {
            InitializeComponent();
            audioManager = AudioManager.Current;
            SetTheme();
            SetDifficulty();
            NewPlayerName();
            InitializingGame();
            LoadingGameHistory();
            EnableInput(false);
        }

        private async void InitializingGame()
        {
            try
            {
                await LoadingWordList();
                NewGame();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                FeedbackLabel.Text = "Game could not have been initialized.";
            }
        }

        //Attempt to allow user to to input their name 
        private async void NewPlayerName()
        {
            if (File.Exists(filePathPlayer))
            {
                var json = File.ReadAllText(filePathPlayer);
                var player = JsonSerializer.Deserialize<Player>(json);
                playerName = player?.Name ?? "Player";
                FeedbackLabel.Text = $"Hi {playerName}! Welcome back!";
                return;
            }

            string name = await DisplayPromptAsync(
                title: "Welcome to Wordle!",
                message: "Enter your name:",
                accept: "OK",
                cancel: "Cancel",
                placeholder: "Player",
                maxLength: 20,
                keyboard: Keyboard.Text
            );

            if (!string.IsNullOrWhiteSpace(name))
            {
                playerName = name;
            }
            else
            {
                playerName = "Player";
            }

            var newPlayer = new Player { Name = playerName };
            var jsonString = JsonSerializer.Serialize(newPlayer);
            File.WriteAllText(filePathPlayer, jsonString);

            FeedbackLabel.Text = $"Hi {playerName}, welcome to Wordle!";
        }

        private void SavingPlayerName()
        {
            var player = new Player { Name = playerName };

            var json = JsonSerializer.Serialize(player);

            File.WriteAllText(filePathPlayer, json);
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

        //Downloads word list
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
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download the list of words: {ex.Message}");
                throw;
            }
        }

        //Reads words from the list
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

        //Selects random word from the list
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

        private void NewGame()
        {
            SetDifficulty();
            SelectingRandomWord();
            attemptsMade = 0;
            Letter1.Text = Letter2.Text = Letter3.Text = Letter4.Text = Letter5.Text = string.Empty;
            EnableInput(true);
        }

        //Allow user to only start typing when they click "New Game" button
        private void EnableInput(bool isEnabled)
        {
            Letter1.IsEnabled = isEnabled;
            Letter2.IsEnabled = isEnabled;
            Letter3.IsEnabled = isEnabled;
            Letter4.IsEnabled = isEnabled;
            Letter5.IsEnabled = isEnabled;
        }

        private void OnGuessSubmit(object sender, EventArgs e)
        {
            string guess = $"{Letter1.Text}{Letter2.Text}{Letter3.Text}{Letter4.Text}{Letter5.Text}".ToUpper();

            if (guess.Length != 5 || !guess.All(char.IsLetter))
            {
                FeedbackLabel.Text = "This is not valid ❌. Please enter a 5 letter word";
                return;
            }

            ProcessGuess(guess);
        }

        //Feedback after entering a guess
        private void ProcessGuess(string guess)
        {
            attemptsMade++;
            char[] feedback = new char[5];
            string[] letterFeedback = new string[5];
            string emojiGrid = "";

            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == chosenWord[i])
                {
                    feedback[i] = '✓';
                    emojiGrid += "🟩";
                    letterFeedback[i] = $"Letter '{guess[i]}' is correct and in the correct position.";
                }
                else if (chosenWord.Contains(guess[i]))
                {
                    feedback[i] = '~';
                    emojiGrid += "🟨";
                    letterFeedback[i] = $"Letter '{guess[i]}' is in the word but in the wrong position.";
                }
                else
                {
                    feedback[i] = 'X';
                    emojiGrid += "⬜";
                    letterFeedback[i] = $"Letter '{guess[i]}' is not in the word.";
                }
            }

            if (guess == chosenWord)
            {
                SavingGameResults(emojiGrid);
            }
            else if (attemptsMade >= attemptsMax)
            {
                SavingGameResults(emojiGrid);
            }

            FeedbackLabel.Text = $"Guess: {guess} - Feedback: {string.Join(" ", feedback)}\n";

            for (int i = 0; i < 5; i++)
            {
                FeedbackLabel.Text += $"{letterFeedback[i]}\n";
            }

            if (guess == chosenWord)
            {
                FeedbackLabel.Text += $"\n🎉 Congratulations! You guessed the word '{chosenWord}' in {attemptsMade} attempts!";
                PlayWinSound();
                EnableInput(false);
            }
            else if (attemptsMade >= attemptsMax)
            {
                FeedbackLabel.Text += $"\n😞 Bad luck! You've used all attempts! The correct word was '{chosenWord}'.";
                EnableInput(false);
            }
            else
            {
                FeedbackLabel.Text += $"\nAttempts remaining: {attemptsMax - attemptsMade}";
            }
            Console.WriteLine(FeedbackLabel.Text);
        }

        //Allow user to click and start a new game
        private void OnNewGame(object sender, EventArgs e)
        {
            NewGame();
            FeedbackLabel.Text = "New Game has started!";
            Letter1.Text = Letter2.Text = Letter3.Text = Letter4.Text = Letter5.Text = string.Empty;
        }

        //Loads game history
        private void LoadingGameHistory()
        {
            if (File.Exists(filePathHistory))
            {
                var json = File.ReadAllText(filePathHistory);
                gameHistory = JsonSerializer.Deserialize<List<GameResult>>(json) ?? new List<GameResult>();
            }
        }

        private void SavingGameHistory()
        {
            File.WriteAllText(filePathHistory, JsonSerializer.Serialize(gameHistory));
        }

        //Saves results, random word, date time, how many attemps were made and emoji letter position
        private void SavingGameResults(string emojiGrid)
        {
            var result = new GameResult(DateTime.Now, chosenWord, attemptsMade, emojiGrid);
            gameHistory.Add(result);
            SavingGameHistory();
        }

        //Bring user to game history page
        private async void OnViewHistory(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistoryPage(gameHistory));
        }

        //Auto move to the next box
        private void OnLetterTextChanged(object sender, TextChangedEventArgs e)
        {
            var input = sender as Entry;
            if (input == null || string.IsNullOrEmpty(e.NewTextValue)) return;

            input.Text = e.NewTextValue.Length > 1 ? e.NewTextValue.Substring(0, 1) : e.NewTextValue;

            if (input == Letter1) Letter2.Focus();
            else if (input == Letter2) Letter3.Focus();
            else if (input == Letter3) Letter4.Focus();
            else if (input == Letter4) Letter5.Focus();
            
        }

        //Brings to the settings page
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        //Allowing user to choose light and dark mode
        private void SetTheme()
        {
            if (Preferences.ContainsKey("isDark"))
            {
                bool isDark = Preferences.Get("isDark", false);
                App.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
            }
            else
            {
                App.Current.UserAppTheme = AppTheme.Light;
            }
        }

        //Allow user to choose difficulty
        private void SetDifficulty()
        {
            string difficulty = Preferences.Get("Difficulty", "Moderate");
            
            switch (difficulty)
            {
                case "Easy":
                    attemptsMax = 12;
                    break;
                case "Moderate":
                    attemptsMax = 6;
                    break;
                case "Hard":
                    attemptsMax = 3;
                    break;
                default:
                    attemptsMax = 6;
                    break;
            }
        }
        //Sound play after guessing the word correctly
        private async void PlayWinSound()
        {
            var audioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("win.mp3"));
            audioPlayer.Play();
        }
    }
}

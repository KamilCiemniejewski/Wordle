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

        public MainPage()
        {
            InitializeComponent();
            LoadingWordList();
        }
        private async void LoadingWordList()
        {
            try
            {
                await InitializingWordListAsync();

                string words = ReadingWordList();
                Console.WriteLine($"The list of words have been loaded with {words.Split("\n").Length} words.");
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task InitializingWordListAsync()
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
                Console.WriteLine("The list of words have been downloaded and saved locally.");
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"Failed to download the list of words: {ex.Message}");
            }
        }

        private string ReadingWordList()
        {
            return File.Exists(filePathLocal) ? File.ReadAllText(filePathLocal) : string.Empty;
        }
    }

}

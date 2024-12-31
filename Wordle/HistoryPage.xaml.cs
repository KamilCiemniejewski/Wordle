using System.Collections.Generic;

namespace Wordle
{
    public partial class HistoryPage : ContentPage
    {
        //Showing previous game results
        public HistoryPage(List<GameResult> gameResults)
        {
            InitializeComponent();
            HistoryListView.ItemsSource = gameResults;
        }
    }
}
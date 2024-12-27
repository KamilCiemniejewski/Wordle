using System.Collections.Generic;

namespace Wordle
{
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage(List<GameResult> gameResults)
        {
            InitializeComponent();
            HistoryListView.ItemsSource = gameResults;
        }
    }
}
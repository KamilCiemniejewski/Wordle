using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordle
{
    public class GameResult
    {
        public DateTime Timestamp { get; set; }
        public string Word { get; set; }
        public int Guesses { get; set; }
        public string EmojiGrid { get; set; }

        public GameResult(DateTime timestamp, string word, int guesses, string emojiGrid = "")
        {
            Timestamp = timestamp;
            Word = word;
            Guesses = guesses;
            EmojiGrid = emojiGrid;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth
{
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public string PathToFile { get; set; }
        public string PathToImage { get; set; }
        public bool IsSongliked { get; set; }
        public string LikeBtnSymb { get; set; }
        public bool IsPlaying { get; set; }
        public int Listens { get; set; }
        public string Genre { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Durat { get; set; }
        public string DisplayText
        {
            get { return Title + " - " + Artist; }
        }

        public Song(string name, string genre)
        {
            Title = name;
            Genre = genre;
        }

        public Song()
        {
            
        }
    }
}

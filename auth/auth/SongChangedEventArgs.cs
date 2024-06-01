using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth
{
    public class SongChangedEventArgs : EventArgs
    {
        public Song NewSong { get; private set; }

        public SongChangedEventArgs(Song newSong)
        {
            NewSong = newSong;
        }
    }

    public class SongPlayer
    {
        private Song _currentSong;

        public event EventHandler<SongChangedEventArgs> SongChanged;

        public Song CurrentSong
        {
            get { return _currentSong; }
            set
            {
                if (_currentSong != value)
                {
                    _currentSong = value;
                    OnSongChanged(new SongChangedEventArgs(value));
                }
            }
        }

        protected virtual void OnSongChanged(SongChangedEventArgs e)
        {
            SongChanged?.Invoke(this, e);
        }
    }
}

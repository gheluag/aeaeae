using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth
{
    public sealed class PlaybackManager
    {
        public static readonly PlaybackManager instance = new PlaybackManager();
        public readonly List<Song> playbackQueue = new List<Song>();
        public int currentSongIndex = -1;
        public int lastPlayedSongIndex = -1;
        public bool newSongSelected { get; set; } = false;


        private PlaybackManager() { }

        public static PlaybackManager Instance
        {
            get { return instance; }
        }

        public void AddSongsToQueue(List<Song> songs)
        {
            playbackQueue.AddRange(songs);
            currentSongIndex = 0;
            lastPlayedSongIndex = -1;
        }

        public void ShuffleQueue()
        {
            Random random = new Random();
            List<Song> shuffledQueue = playbackQueue.OrderBy(x => random.Next()).ToList();
            playbackQueue.Clear();
            playbackQueue.AddRange(shuffledQueue);
        }

        public void ClearQueue()
        {
            playbackQueue.Clear();
            currentSongIndex = -1;
            lastPlayedSongIndex = -1;
        }

        public Song GetCurrentSong()
        {
            if (currentSongIndex >= 0 && currentSongIndex < playbackQueue.Count)
            {
                return playbackQueue[currentSongIndex];
            }
            return null;
        }

        public Song GetNextSong()
        {
            if (playbackQueue.Count > 0)
            {
                if (currentSongIndex == playbackQueue.Count - 1)
                {
                    currentSongIndex = 0;
                    lastPlayedSongIndex = currentSongIndex;
                    return playbackQueue[currentSongIndex];
                }
                else
                {
                    currentSongIndex++;
                    lastPlayedSongIndex = currentSongIndex;
                    return playbackQueue[currentSongIndex];
                }
            }
            return null;
        }

        public Song GetPreviousSong()
        {
            if (playbackQueue.Count > 0)
            {
                if (currentSongIndex == 0)
                {
                    currentSongIndex = playbackQueue.Count - 1;
                    lastPlayedSongIndex = currentSongIndex;
                    return playbackQueue[currentSongIndex];
                }
                else
                {
                    currentSongIndex--;
                    lastPlayedSongIndex = currentSongIndex;
                    return playbackQueue[currentSongIndex];
                }
            }
            return null;
        }

        public void SetCurrentSongIndex(int index)
        {
            currentSongIndex = index;
        }

    }


}



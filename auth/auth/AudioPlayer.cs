using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace auth
{
    public class AudioPlayer
    {

        public MediaPlayer mediaPlayer;

        public AudioPlayer()
        {
            mediaPlayer = new MediaPlayer();
        }

        public void Play(string filePath)
        {
            Song sn = new Song();
            filePath = sn.PathToFile;
            mediaPlayer.Open(new Uri(filePath, UriKind.RelativeOrAbsolute));
            mediaPlayer.Play();
        }

        public void Pause()
        {
            mediaPlayer.Pause();
        }

        public void Stop()
        {
            mediaPlayer.Stop();
        }

        public bool IsPlaying
        {
            get { return mediaPlayer.Clock != null; }
        }
    }
}

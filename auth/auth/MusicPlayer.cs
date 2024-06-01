using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using YoutubeExplode;

namespace auth
{
    public class MusicPlayer
    {
        private static MusicPlayer instance;
        public static MusicPlayer Instance
        {
            get
            {
                if (instance == null)
                    instance = new MusicPlayer();

                return instance;
            }
        }

        public MediaPlayer mediaPlayer;
        public Song CurrentSong { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        private int currentSongId;

        public event EventHandler<EventArgs> MediaEnded;

        public TimeSpan Duration { get; private set; }
        public TimeSpan Position
        {
            get { return mediaPlayer.Position; }
            set
            {
                mediaPlayer.Position = value;
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler PositionChanged;

        private DispatcherTimer positionTimer;

        public MusicPlayer()
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            positionTimer = new DispatcherTimer();
            positionTimer.Interval = TimeSpan.FromMilliseconds(200); // Обновление каждые 200 мс
            positionTimer.Tick += PositionTimer_Tick;
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Position = mediaPlayer.Position;
            }
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            Duration = mediaPlayer.NaturalDuration.HasTimeSpan ? mediaPlayer.NaturalDuration.TimeSpan : TimeSpan.Zero;
            PositionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            Stop();
            MediaEnded?.Invoke(this, EventArgs.Empty);
            PositionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Play()
        {
            if (CurrentSong == null)
            {
                return;
            }

            if (IsPaused && currentSongId != CurrentSong.Id)
            {
                Stop();
                string audioFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "songs");
                string audioFilePath = Path.Combine(audioFolder, CurrentSong.PathToFile);
                mediaPlayer.Open(new Uri(audioFilePath));
                mediaPlayer.Play();
                currentSongId = CurrentSong.Id;
                IsPlaying = true;
                IsPaused = false;
                positionTimer.Start();
            }
            else if (IsPaused)
            {
                Resume();
            }
            else
            {
                string audioFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "songs");
                string audioFilePath = Path.Combine(audioFolder, CurrentSong.PathToFile);
                mediaPlayer.Open(new Uri(audioFilePath));
                mediaPlayer.Play();
                currentSongId = CurrentSong.Id;
                IsPlaying = true;
                IsPaused = false;
                positionTimer.Start();
            }
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                mediaPlayer.Pause();
                IsPlaying = false;
                IsPaused = true;
                positionTimer.Stop();
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                mediaPlayer.Play();
                IsPlaying = true;
                IsPaused = false;
                positionTimer.Start();
            }
        }

        public void Stop()
        {
            mediaPlayer.Stop();
            IsPlaying = false;
            IsPaused = false;
            currentSongId = 0;
            positionTimer.Stop();
            Duration = TimeSpan.Zero;
            Position = TimeSpan.Zero;
        }

        private double _volume = 1.0; // Начальное значение громкости
        public double Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                mediaPlayer.Volume = value; // Устанавливаем громкость MediaPlayer
            }
        }

        public void LoadVolume()
        {
            Volume = Properties.Settings.Default.Volume;
        }

        public void SaveVolume()
        {
            Properties.Settings.Default.Volume = Volume;
            Properties.Settings.Default.Save();
        }
    }

}


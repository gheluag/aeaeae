using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Configuration;

namespace auth
{
    /// <summary>
    /// Логика взаимодействия для homepage.xaml
    /// </summary>
    public partial class homepage : Page
    {
        MusicPlayer musicPlayer = MusicPlayer.Instance;
        DataBase db = new DataBase();
        private SongPlayer _songPlayer;

        public homepage()
        {
            InitializeComponent();

            musicPlayer = MusicPlayer.Instance;
            _songPlayer = new SongPlayer();
            _songPlayer.SongChanged += SongPlayer_SongChanged;

            LoadCurrentSongFromDatabase();

            MusicPlayer.Instance.MediaEnded += MusicPlayer_MediaEnded;
            MusicPlayer.Instance.PositionChanged += Instance_PositionChanged;
        }

        private void artistname_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            Utils.SetUnderlineAndHandCursor(textBlock);
        }

        private void artistname_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            Utils.RemoveUnderlineAndSetArrowCursor(textBlock);
        }

        private void artistname_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            Utils.OpenArtistPage((TextBlock)sender, NavigationService, database);
        }


        private void MusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            Song nextSong = PlaybackManager.Instance.GetNextSong();

            if (nextSong != null)
            {
                MusicPlayer.Instance.CurrentSong = nextSong;
                _songPlayer.CurrentSong = nextSong;
                MusicPlayer.Instance.Play();

                int userId = database.GetUserIdByUsername(CurrentUser.Username);
                database.addlistens(nextSong.Id);
                int genreId = database.GetGenreId(nextSong.Genre);
                int artistId = database.GetArtistIdByName(nextSong.Artist);
                database.AddListenedSong(genreId, userId, nextSong.Id, artistId);

                database.InsertOrUpdateCurrentSong(nextSong);
            }
            else
            {
                MusicPlayer.Instance.Stop();
                playStop_btn.Content = new TextBlock() { Text = "▶️" };
            }
        }

        private void SongPlayer_SongChanged(object sender, SongChangedEventArgs e)
        {
            Utils.Update_SongChanged(albumArtImage, songTitleTextBlock, artistTextBlock, e);
        }

        private void PlayStop_Click(object sender, RoutedEventArgs e)
        {
            Utils.HandlePlayPauseButton(playStop_btn);
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {

            profilepage profilePage = new profilepage(CurrentUser.Username);
            NavigationService.Navigate(profilePage);
        }
        private void gosearch(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToSearchPage(NavigationService);
        }
        private void OpenLikedPlaylist(object sender, MouseButtonEventArgs e)
        {
            Utils.OpenLikedPlaylist(NavigationService, database);
        }
        private void OpenCreatePlaylistWindow(object sender, RoutedEventArgs e)
        {
            Utils.OpenCreatePlaylistWindow(playlists, PlaylistListBox, database);
        }

        // плейлист

        private ObservableCollection<string> playlists;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            DataBase db = new DataBase();
            int userId = db.GetUserIdByUsername(CurrentUser.Username);
            List<string> playlistNames = db.GetUserPlaylists(userId);

            playlists = new ObservableCollection<string>(playlistNames);

            PlaylistListBox.ItemsSource = playlists;

            Utils.LoadPageData(database, listBoxLikedArtists, listBoxLikedAlbums, playStop_btn);

            bool isTodaySongExists = db.IsTodaySongExist();

            if (!isTodaySongExists)
            {
                Song randomSong = db.GetRandomSong();

                db.SaveSongToView(randomSong);
            }

            UpdateUIWithTodaySong();

        }

        // открыть плейлист
        private void OpenPlaylistPage(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            Utils.NavigateToPlaylistPage(textBlock, NavigationService, database);
        }
        // удалить пл

        private void DeletePlaylist(object sender, MouseButtonEventArgs e)
        {
            Utils.DeletePlaylist((TextBlock)sender, PlaylistListBox, database);
        }

        // artist page
        private void OpenArtistPage(object sender, MouseButtonEventArgs e)
        {
            Utils.OpenArtistPage((TextBlock)sender, NavigationService, database);
        }
        // страница альбомов

        private void OpenAlbumPage(object sender, MouseButtonEventArgs e)
        {
            Utils.OpenAlbumPage((TextBlock)sender, NavigationService, database);
        }
        // novinki

        private void NewReleasesBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NewSongsPage playlistWindow = new NewSongsPage();
            NavigationService.Navigate(playlistWindow);
        }

        // предпочтения

        private void Wishesplaylist_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PreferencePlaylist pp = new PreferencePlaylist();
            NavigationService.Navigate(pp);
        }

        // song of the day

        private void UpdateUIWithTodaySong()
        {
            Song todaySong = db.GetCurrentTodaySongFromView();

            if (todaySong != null)
            {
                songNameTextBlock.Text = todaySong.Title;
                artistNameTextBlock.Text = todaySong.Artist;
                string songfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");
                string songFilePath = Path.Combine(songfolder, todaySong.PathToImage);
                songImage.Source = new BitmapImage(new Uri(songFilePath));
            }
        }

        DataBase database = new DataBase();
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlaybackManager.Instance.ClearQueue();

            Song currentSong = db.GetCurrentTodaySongFromView();
            PlaybackManager.Instance.AddSongsToQueue(new List<Song> { currentSong });
            musicPlayer.CurrentSong = currentSong;
            _songPlayer.CurrentSong = currentSong;
            musicPlayer.Play();
            playStop_btn.Content = new TextBlock() { Text = "⏸" };
            db.addlistens(currentSong.Id);
            int userid = db.GetUserIdByUsername(CurrentUser.Username);
            int genre_id = db.GetGenreId(currentSong.Genre);
            int artistid = db.GetArtistIdByName(currentSong.Artist);
            db.AddListenedSong(genre_id, userid, currentSong.Id, artistid);

            database.InsertOrUpdateCurrentSong(currentSong);

        }

        public void LoadCurrentSongFromDatabase()
        {
            Song currentSong = database.GetCurrentSong();

            if (currentSong != null)
            {
                _songPlayer.CurrentSong = currentSong;
            }
        }


        private void NextSongButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.HandleNextSongButtonClick(musicPlayer, _songPlayer, database);
        }

        private void PreviousSongButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.HandlePreviousSongButtonClick(musicPlayer, _songPlayer, database);
        }


        private void songProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Utils.HandleSongProgressValueChanged(songProgress, e.NewValue);
        }

        private void Instance_PositionChanged(object sender, EventArgs e)
        {
            Utils.HandlePlayerPositionChanged(songProgress);
        }


    }
}

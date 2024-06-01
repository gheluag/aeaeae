using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;

namespace auth
{
    /// <summary>
    /// Логика взаимодействия для PlaylistPage.xaml
    /// </summary>
    public partial class PlaylistPage : Page
    {
        private int playlistId;
        DataBase database = new DataBase();
        MusicPlayer musicPlayer = MusicPlayer.Instance;
        private SongPlayer _songPlayer;

        public PlaylistPage(int playlistId)
        {
            InitializeComponent();
            this.playlistId = playlistId;
            LoadPlaylist(playlistId);
            musicPlayer = MusicPlayer.Instance;
            _songPlayer = new SongPlayer();
            _songPlayer.SongChanged += SongPlayer_SongChanged;

            LoadCurrentSongFromDatabase();

            MusicPlayer.Instance.MediaEnded += MusicPlayer_MediaEnded;

            MusicPlayer.Instance.PositionChanged += Instance_PositionChanged;
        }

        private void MusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            Utils.HandleMediaEnded(_songPlayer, playStop_btn, database);
        }

        private void SongPlayer_SongChanged(object sender, SongChangedEventArgs e)
        {
            Utils.Update_SongChanged(albumArtImage, songTitleTextBlock, artistTextBlock, e);
        }



        private void PlaylistPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlaylist(playlistId);
        }

        List<Song> playlistsongs;
        private void LoadPlaylist(int playlistId)
        {
            string songfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");
            Playlists playlist = database.GetPlaylist(playlistId);

            if (playlist != null)
            {
                DataContext = playlist;

                playlistsongs = database.GetPlaylistSongs(playlistId);
                SongsListBox.ItemsSource = playlistsongs;

                foreach (var song in playlistsongs)
                {
                    song.IsSongliked = database.IsSongliked(database.GetUserIdByUsername(CurrentUser.Username), song.Id);
                    song.LikeBtnSymb = song.IsSongliked ? "♥️" : "♡";
                    string songFilePath = Path.Combine(songfolder, song.PathToImage);
                    song.PathToImage = songFilePath;
                }

                PlaylistNameTextBlock.Text = playlist.Name;

                if (PlaybackManager.Instance.newSongSelected || PlaybackManager.Instance.playbackQueue.Count == 0)
                {
                    PlaybackManager.Instance.ClearQueue();
                    PlaybackManager.Instance.AddSongsToQueue(playlistsongs);
                    PlaybackManager.Instance.newSongSelected = false;
                }

            }
        }
        private void LikeButton_song(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Song song = (Song)button.DataContext;
            Utils.HandleLikeButtonClick(button, song, database);
        }

        // панель боковая
        private void gohome(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToHomePage(NavigationService);
        }

        private void gosearch(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToSearchPage(NavigationService);
        }


        private void PlayStop_Click(object sender, RoutedEventArgs e)
        {
            Utils.HandlePlayPauseButton(playStop_btn);
        }

        private void OpenLikedPlaylist(object sender, MouseButtonEventArgs e)
        {
            Utils.OpenLikedPlaylist(NavigationService, database);
        }

        // pl page

        private void OpenPlaylistPage(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            Utils.NavigateToPlaylistPage(textBlock, NavigationService, database);
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
        }



        // удалить пл

        private void DeletePlaylist(object sender, MouseButtonEventArgs e)
        {
            DataBase db = new DataBase();
            string playlistName = ((TextBlock)sender).Text;
            int playlistId = db.GetPlaylistIdByName(playlistName);
            int userId = db.GetUserIdByUsername(CurrentUser.Username);

            if (playlistId != -1)
            {
                MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить плейлист?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    db.DeletePlaylist(playlistId);
                    PlaylistListBox.ItemsSource = db.GetUserPlaylists(userId);
                }
            }
        }


        // menu

        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Song song = button.DataContext as Song;
            Utils.OpenContextMenu(button, song);
        }

        private void RemoveFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var song = (sender as MenuItem).DataContext as Song;
            DataBase db = new DataBase();
            db.RemoveFromPlaylist(playlistId, song.Id);
            UpdatePlaylist();
        }

        private void UpdatePlaylist()
        {
            DataBase db = new DataBase();
            var songs = db.GetPlaylistSongs(playlistId);

            SongsListBox.ItemsSource = songs;
        }
        private void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Utils.AddToPlaylist(sender as MenuItem, database);
        }
        private void SongSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
           
                PlaybackManager.Instance.ClearQueue();
                PlaybackManager.Instance.AddSongsToQueue(playlistsongs);
                PlaybackManager.Instance.newSongSelected = false;
            Utils.HandleSongSelectionChanged(listBox, musicPlayer, _songPlayer, playStop_btn, database);
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


        private bool isMixButtonChecked = false;

        private void mix_btn_Checked(object sender, RoutedEventArgs e)
        {
            isMixButtonChecked = true;
            PlaybackManager.Instance.ShuffleQueue();
        }

        private void mix_btn_Unchecked(object sender, RoutedEventArgs e)
        {
            isMixButtonChecked = false;
            PlaybackManager.Instance.ClearQueue();
            PlaybackManager.Instance.AddSongsToQueue(playlistsongs);
            PlaybackManager.Instance.newSongSelected = false;

            Song currentSong = PlaybackManager.Instance.GetCurrentSong();
            if (currentSong != null)
            {
                SongSelectionChanged(null, null);
            }

        }
    }
}
